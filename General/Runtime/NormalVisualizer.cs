using System;
using UnityEditor;
using UnityEngine;

namespace FortySevenE
{
    public class NormalVisualizer : MonoBehaviour
    {
        public bool showNormal;
        public bool showVertexIndex;
        public bool alwaysShow;
        public Color color = Color.yellow;
        public float normalsLength = 1f;

        private MeshFilter _meshFilter;
        private SkinnedMeshRenderer _smr;
        private Mesh _smrBakedMesh;

        private Mesh GetMesh()
        {
            if (_meshFilter != null) return _meshFilter.sharedMesh;

            if (_smr != null)
            {
                if (_smrBakedMesh == null) _smrBakedMesh = new Mesh();
                _smr.BakeMesh(_smrBakedMesh);
                if (_smrBakedMesh == null)
                {
                    Debug.LogWarning("Invalid Skinned Mesh");
                }

                return _smrBakedMesh;
            }

            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter != null)
            {
                return _meshFilter.sharedMesh;
            }

            _smr = GetComponent<SkinnedMeshRenderer>();
            if (_smr != null)
            {
                if (_smrBakedMesh == null) _smrBakedMesh = new Mesh();
                _smr.BakeMesh(_smrBakedMesh);
                return _smrBakedMesh;
            }

            return null;
        }

        private void OnDrawGizmos()
        {
            if (alwaysShow)
            {
                OnDrawGizmosSelected();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Mesh mesh = GetMesh();
            if (mesh == null) return;

            var defaultColor = Handles.color;
            Handles.matrix = transform.localToWorldMatrix;
            Handles.color = color;
            var verts = mesh.vertices;
            var normals = mesh.normals;
            int len = mesh.vertexCount;

            if (showNormal)
            {
                for (int i = 0; i < len; i++)
                {
                    if (i < normals.Length)
                    {
                        Handles.DrawLine(verts[i], verts[i] + normals[i] * normalsLength);
                    }
                }
            }

            if (showVertexIndex)
            {
                for (int i = 0; i < len; i++)
                {
                    Handles.Label(verts[i] + normals[i] * normalsLength, $"{i}");
                }
            }

            Handles.color = defaultColor;
        }
    }
}