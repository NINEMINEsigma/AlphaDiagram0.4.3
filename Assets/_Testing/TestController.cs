using UnityEngine;
using AD.Utility;

namespace TTT
{

    public class TestController : MonoBehaviour
    {
        public MeshFilter meshFilter;

        public CustomCurveSource Curve = new();
        public AnimationCurve Size = new();

        private void Start()
        {
            meshFilter.mesh = new Mesh();
            InitMesh();
        }

        private void OnValidate()
        {
            InitMesh();
        }

        private void InitMesh()
        {
            MeshExtension.InitMesh(meshFilter, MeshExtension.GenerateCurveMeshData(Curve, MeshExtension.BuildNormalType.JustDirection, Vector3.right, Size));
        }
    }
}
