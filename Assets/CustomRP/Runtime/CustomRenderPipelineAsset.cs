using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    /// <summary>
    /// 资产文件，在 ProjectSettings -> Graphics -> ScriptableRenderPipelineSettings 中设置
    /// </summary>
    [CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline", fileName = "CustomRenderPipelineAsset")]
    public class CustomRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            Debug.Log("CustomRenderPipelineAsset.CreatePipeline");
            return new CustomRenderPipeline();
        }
    }
}