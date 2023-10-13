using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRP.Runtime
{
    public partial class CameraRenderer
    {
        partial void PrepareForSceneWindow();
        partial void DrawUnsupportedShaders();
        partial void DrawGizmos();
        
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

        // 将 UI 几何图形发送到场景视图中进行渲染
        partial void PrepareForSceneWindow()
        {
            if (_camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
            }
        }
        
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

        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
                _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
            }
        }
#endif
    }
}