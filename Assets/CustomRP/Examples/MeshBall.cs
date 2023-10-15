﻿using UnityEngine;
using Random = UnityEngine.Random;

namespace CustomRP.Examples
{
    public class MeshBall : MonoBehaviour
    {
        private static int _baseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        private MaterialPropertyBlock _block;
        private Matrix4x4[] _matrices = new Matrix4x4[1023];
        private Vector4[] _baseColors = new Vector4[1023];

        private void Awake()
        {
            for (int i = 0; i < _matrices.Length; ++i)
            {
                _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one);
                _baseColors[i] = new Vector4(Random.value, Random.value, Random.value, 1.0f);
            }
        }

        private void Update()
        {
            if (_block == null)
            {
                _block = new MaterialPropertyBlock();
                _block.SetVectorArray(_baseColorId, _baseColors);
            }
            
            Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, 1023, _block);
        }
    }
}