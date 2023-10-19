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
        private static int _dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
        
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
        private static Matrix4x4[] _dirShadowMatrices = new Matrix4x4[MaxShadowedDirectionalLightCount];

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

            _buffer.SetGlobalMatrixArray(_dirShadowMatricesId, _dirShadowMatrices);
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
            _dirShadowMatrices[index] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix,
                SetTileViewport(index, split, tileSize), split);
            _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            
            ExecuteBuffer();
            _context.DrawShadows(ref shadowDrawingSettings);
        }
        
        Vector2 SetTileViewport(int index, int split, float tileSize) 
        {
            Vector2 offset = new Vector2(index % split, index / split);
            _buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
            return offset;
        }
        
        // 返回从世界空间转换为阴影图块空间的矩阵
        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split) 
        {
            // 判断 z buffer 是否反向
            if (SystemInfo.usesReversedZBuffer) 
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }
            
            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            
            return m;
        }

        public void Cleanup()
        {
            _buffer.ReleaseTemporaryRT(_dirShadowAtlasId);
            ExecuteBuffer();
        }
    }
}