using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class CustomRenderPipeline : RenderPipeline
    {
        private readonly CameraRenderer _cameraRenderer = new CameraRenderer();

        public CustomRenderPipeline()
        {
            // 是否启用 SRP Batch
            GraphicsSettings.useScriptableRenderPipelineBatching = false;
        }
        
        // 每帧调用
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera camera in cameras)
            {
                _cameraRenderer.Render(context, camera);
            }
        }
    }
}