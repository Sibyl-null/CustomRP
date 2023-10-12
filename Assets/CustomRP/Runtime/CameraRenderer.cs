using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public class CameraRenderer
    {
        private const string BufferName = "Render Camera";

        private ScriptableRenderContext _context;
        private Camera _camera;
        
        // 某些任务(例如绘制天空盒)可以通过专用方法发出，但其他命令必须通过单独的命令缓冲区间接发出
        private readonly CommandBuffer _buffer = new CommandBuffer()
        {
            name = BufferName
        };

        // 绘制相机可以看到的所有几何图形
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            _context = context;
            _camera = camera;

            Setup();
            DrawVisibleGeometry();
            Submit();
        }

        private void Setup()
        {
            // 设置相机特定的全局着色器变量. 例如视图投影矩阵 unity_MatrixVP
            // 在 FrameDebugger 中的 ShaderProperties 中可见
            _context.SetupCameraProperties(_camera);
            
            // 清除深度缓冲和颜色缓冲. 在设置相机属性之后调用, 效率更高
            _buffer.ClearRenderTarget(true, true, Color.clear);
            _buffer.BeginSample(BufferName);
            ExecuteBuffer();
        }

        private void DrawVisibleGeometry()
        {
            // 将相机传递给 DrawSkybox，仅用于确定是否应该绘制天空盒，这是通过相机的清除标志控制的
            _context.DrawSkybox(_camera);
        }
        
        private void Submit()
        {
            _buffer.EndSample(BufferName);
            ExecuteBuffer();
            
            // 向上下文发出的命令被缓冲了. 必须通过调用 Submit 来提交排队的工作以供执行
            _context.Submit();
        }

        private void ExecuteBuffer()
        {
            // 这会从缓冲区复制命令但不会清除它. 该方法也不会立刻执行，而是会等到 Submit 时再执行
            _context.ExecuteCommandBuffer(_buffer);
            // 手动清除以供下一次使用
            _buffer.Clear();
        }
    }
}