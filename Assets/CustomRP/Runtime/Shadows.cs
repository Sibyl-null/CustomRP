using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }
    
    public class Shadows
    {
        private const string BufferName = "Shadows";
        private const int MaxShadowedDirectionalLightCount = 4;
        private static int _dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
        
        private readonly CommandBuffer _buffer = new CommandBuffer 
        {
            name = BufferName
        };

        private ScriptableRenderContext _context;
        private CullingResults _cullingResults;
        private ShadowSettings _shadowSettings;

        private int _shadowedDirectionalLightCount = 0;
        private readonly ShadowedDirectionalLight[] _shadowedDirectionalLights =
            new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings) 
        {
            _context = context;
            _cullingResults = cullingResults;
            _shadowSettings = settings;

            _shadowedDirectionalLightCount = 0;
        }

        private void ExecuteBuffer() 
        {
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
        {
            if (_shadowedDirectionalLightCount < MaxShadowedDirectionalLightCount
                && light.shadows != LightShadows.None && light.shadowStrength > 0f
                && _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                _shadowedDirectionalLights[_shadowedDirectionalLightCount] = new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex
                };
                ++_shadowedDirectionalLightCount;
            }
        }

        public void Render()
        {
            if (_shadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
            else
            {
                // 不需要阴影时获取 1×1 虚拟纹理
                _buffer.GetTemporaryRT(_dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear,
                    RenderTextureFormat.Shadowmap);
            }
        }

        private void RenderDirectionalShadows()
        {
            int atlasSize = (int)_shadowSettings.directional.atlasSize;
            _buffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear,
                RenderTextureFormat.Shadowmap);
            _buffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            _buffer.ClearRenderTarget(true, false, Color.clear);
            
            _buffer.BeginSample(BufferName);
            ExecuteBuffer();

            int split = _shadowedDirectionalLightCount <= 1 ? 1 : 2;
            int tileSize = atlasSize / split;
            
            for (int i = 0; i < _shadowedDirectionalLightCount; ++i)
                RenderDirectionalShadows(i, split, tileSize);
            
            _buffer.EndSample(BufferName);
            ExecuteBuffer();
        }

        private void RenderDirectionalShadows(int index, int split, int tileSize)
        {
            ShadowedDirectionalLight light = _shadowedDirectionalLights[index];
            ShadowDrawingSettings shadowDrawingSettings =
                new ShadowDrawingSettings(_cullingResults, light.visibleLightIndex);
            
            _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
                light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
                out ShadowSplitData splitData
            );

            shadowDrawingSettings.splitData = splitData;
            SetTileViewport(index, split, tileSize);
            _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            
            ExecuteBuffer();
            _context.DrawShadows(ref shadowDrawingSettings);
        }
        
        void SetTileViewport(int index, int split, float tileSize) 
        {
            Vector2 offset = new Vector2(index % split, index / split);
            _buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        }

        public void Cleanup()
        {
            _buffer.ReleaseTemporaryRT(_dirShadowAtlasId);
            ExecuteBuffer();
        }
    }
}