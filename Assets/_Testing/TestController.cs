using UnityEngine;
using AD.Utility;
using AD;

namespace TTT
{
    //[ExecuteAlways]
    public class TestController : MonoBehaviour
    {
        public MeshFilter meshFilter;

        public MeshExtension.VertexEntry[] vertices;

        public MeshExtension.MeshData meshData;

        private void Start()
        {
            meshData=meshFilter.InitMesh(vertices);
        }

        private void OnValidate()
        {
            meshData=meshFilter.InitMesh(vertices);
        }
    }
}
