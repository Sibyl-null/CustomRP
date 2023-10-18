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
        private const int MaxShadowedDirectionalLightCount = 1;
        
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
    }
}