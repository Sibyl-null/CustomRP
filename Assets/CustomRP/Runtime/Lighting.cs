using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class Lighting
    {
        private const string BufferName = "Lighting";
        private const int MaxDirLightCount = 4;
        
        private static int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
        private static int _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        private static int _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

        private static Vector4[] _dirLightColors = new Vector4[MaxDirLightCount];
        private static Vector4[] _dirLightDirections = new Vector4[MaxDirLightCount];
        
        private CullingResults _cullingResults;
        private readonly CommandBuffer _buffer = new CommandBuffer()
        {
            name = BufferName
        };

        private Shadows _shadows = new Shadows();

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            _cullingResults = cullingResults;
            
            _buffer.BeginSample(BufferName);
            _shadows.Setup(context, cullingResults, shadowSettings);
            SetupLight();
            _buffer.EndSample(BufferName);
            
            context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        private void SetupLight()
        {
            NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;

            int dirLightCount = 0;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                VisibleLight visibleLight = visibleLights[i];

                if (visibleLight.lightType == LightType.Directional)
                {
                    SetupDirectionalLight(dirLightCount++, ref visibleLight);
                    if (dirLightCount >= MaxDirLightCount)
                        break;
                }
            }
            
            _buffer.SetGlobalInt(_dirLightCountId, visibleLights.Length);
            _buffer.SetGlobalVectorArray(_dirLightColorsId, _dirLightColors);
            _buffer.SetGlobalVectorArray(_dirLightDirectionsId, _dirLightDirections);
        }

        private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            _dirLightColors[index] = visibleLight.finalColor;
            _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        }
    }
}