using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class CameraRenderer
    {
        partial void DrawUnsupportedShaders();
        
#if UNITY_EDITOR
        private static Material _errorMaterial;

        private static readonly ShaderTagId[] LegacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        
        partial void DrawUnsupportedShaders()
        {
            if (_errorMaterial == null)
                _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

            DrawingSettings drawingSettings = new DrawingSettings(LegacyShaderTagIds[0], new SortingSettings(_camera))
            {
                overrideMaterial = _errorMaterial
            };
            for (int i = 1; i < LegacyShaderTagIds.Length; ++i)
                drawingSettings.SetShaderPassName(i, LegacyShaderTagIds[i]);
            FilteringSettings filteringSettings = FilteringSettings.defaultValue;
            
            _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        }
#endif
        
    }
}