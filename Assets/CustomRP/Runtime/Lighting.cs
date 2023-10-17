using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class Lighting
    {
        private const string BufferName = "Lighting";
        private static int DirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
        private static int DirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

        private readonly CommandBuffer _buffer = new CommandBuffer()
        {
            name = BufferName
        };

        public void Setup(ScriptableRenderContext context)
        {
            _buffer.BeginSample(BufferName);
            SetupDirectionalLight();
            _buffer.EndSample(BufferName);
            
            context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        private void SetupDirectionalLight()
        {
            Light light = RenderSettings.sun;
            // 为所有使用该属性的 shader 设置值
            _buffer.SetGlobalVector(DirLightColorId, light.color.linear * light.intensity);
            _buffer.SetGlobalVector(DirLightDirectionId, -light.transform.forward);
        }
    }
}