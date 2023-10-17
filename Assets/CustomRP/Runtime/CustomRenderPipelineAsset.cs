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
        [SerializeField] private bool _useDynamicBatching = true;
        [SerializeField] private bool _useGPUInstance = true;
        [SerializeField] private bool _useSRPBatcher = true;

        protected override RenderPipeline CreatePipeline()
        {
            Debug.Log("CustomRenderPipelineAsset.CreatePipeline");
            return new CustomRenderPipeline(_useDynamicBatching, _useGPUInstance, _useSRPBatcher);
        }
    }
}