using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class CameraRenderer
    {
        private const string BufferName = "Render Camera";
        private static readonly ShaderTagId UnlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private static readonly ShaderTagId LitShaderTagId = new ShaderTagId("CustomLit");

        private ScriptableRenderContext _context;
        private Camera _camera;
        private CullingResults _cullingResults;
        private Lighting _lighting = new Lighting();
        
        // 某些任务(例如绘制天空盒)可以通过专用方法发出，但其他命令必须通过单独的命令缓冲区间接发出
        private readonly CommandBuffer _buffer = new CommandBuffer();

        public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, 
            bool useGPUInstance, ShadowSettings shadowSettings)
        {
            _context = context;
            _camera = camera;

            PrepareBuffer();
            PrepareForSceneWindow();    // 可能会向场景添加几何图形，需要在剔除之前完成
            
            if (Cull(shadowSettings.maxDistance) == false)
                return;

            _buffer.BeginSample(SampleName);
            ExecuteBuffer();
            _lighting.Setup(context, _cullingResults, shadowSettings);
            _buffer.EndSample(SampleName);
            
            Setup();
            DrawVisibleGeometry(useDynamicBatching, useGPUInstance);
            DrawUnsupportedShaders();   // Only Editor
            DrawGizmos();               // Only Editor
            _lighting.Cleanup();
            Submit();
        }

        private bool Cull(float maxDistance)
        {
            // 尝试获取 ScriptableCullingParameters 相机剔除参数
            if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                // 在摄像机视角中生成阴影的最大距离, 超过 shadowDistance 的物体不会在游戏中产生阴影
                p.shadowDistance = Mathf.Min(maxDistance, _camera.farClipPlane);
                // 实际剔除调用
                _cullingResults = _context.Cull(ref p);
                return true;
            }

            // 相机为 null 或不可用，设置为非渲染状态，远裁剪平面距离小于近裁剪平面距离等等情况下，会返回 false
            return false;
        }

        private void Setup()
        {
            // 设置相机特定的全局着色器变量. 例如视图投影矩阵 unity_MatrixVP
            // 在 FrameDebugger 中的 ShaderProperties 中可见
            _context.SetupCameraProperties(_camera);
            CameraClearFlags flags = _camera.clearFlags;
            
            // 清除深度缓冲和颜色缓冲. 在设置相机属性之后调用, 效率更高
            _buffer.ClearRenderTarget(
                flags <= CameraClearFlags.Depth,
                flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
            
            _buffer.BeginSample(SampleName);
            ExecuteBuffer();
        }

        private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstance)
        {
            // 1. 渲染不透明物体
            SortingSettings sortingSettings = new SortingSettings(_camera)
            {
                criteria = SortingCriteria.CommonOpaque   // 不透明对象的典型排序，大体上从前往后
            };
            DrawingSettings drawingSettings = new DrawingSettings(UnlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstance
            };
            drawingSettings.SetShaderPassName(1, LitShaderTagId);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
            
            // 2. 将相机传递给 DrawSkybox，仅用于确定是否应该绘制天空盒，这是通过相机的清除标志控制的
            _context.DrawSkybox(_camera);

            // 3. 渲染透明物体，防止被天空盒覆盖. 因为透明物体不写入深度缓存
            sortingSettings.criteria = SortingCriteria.CommonTransparent;   // 透明对象的典型排序，大体上从后往前
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.transparent;
            
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }

        private void Submit()
        {
            _buffer.EndSample(SampleName);
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