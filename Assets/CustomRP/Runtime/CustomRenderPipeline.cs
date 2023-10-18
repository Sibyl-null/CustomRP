using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private readonly CameraRenderer _cameraRenderer = new CameraRenderer();

        private bool _useDynamicBatching;
        private bool _useGPUInstance;
        private ShadowSettings _shadowSettings;

        public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstance, bool useSRPBatcher, ShadowSettings shadowSettings)
        {
            _useDynamicBatching = useDynamicBatching;
            _useGPUInstance = useGPUInstance;
            _shadowSettings = shadowSettings;
            
            // 是否启用 SRP Batch
            GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }
        
        // 每帧调用
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera camera in cameras)
            {
                _cameraRenderer.Render(context, camera, _useDynamicBatching, _useGPUInstance, _shadowSettings);
            }
        }
    }
}