using UnityEngine;

namespace CustomRP.Examples
{
    [DisallowMultipleComponent]
    public class PerObjectMaterialProperties : MonoBehaviour
    {
        private static int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static MaterialPropertyBlock _block;
        
        [SerializeField] private Color _baseColor = Color.white;

        private void OnValidate()
        {
            if (_block == null)
                _block = new MaterialPropertyBlock();
            
            _block.SetColor(_baseColorId, _baseColor);
            GetComponent<Renderer>().SetPropertyBlock(_block);
        }

        private void Awake()
        {
            OnValidate();
        }
    }
}
