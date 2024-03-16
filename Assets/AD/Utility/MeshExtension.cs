using System;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public static class MeshExtension
    {
        public enum BuildNormalType
        {
            Default, JustDirection
        }

        [Serializable]
        public class VertexEntry
        {
            public Vector3 Position = Vector3.zero;
            public Vector3 Normal = Vector3.left;
            public float Size = 1;
            public BuildNormalType Type = BuildNormalType.Default;
        }

        [Serializable]
        public struct MeshData
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uvs;
        }

        public static MeshData RebuildMesh(this MeshFilter mesh, IEnumerable<VertexEntry> SourcePairs)
        {
            return InitMesh(mesh, mesh.sharedMesh, SourcePairs);
        }

        public static MeshData InitMesh(this MeshFilter meshFilter, IEnumerable<VertexEntry> SourcePairs)
        {
            return InitMesh(meshFilter, new Mesh(), SourcePairs);
        }

        public static MeshData InitMesh(this MeshFilter meshFilter, Mesh mesh, IEnumerable<VertexEntry> SourcePairs)
        {
            mesh.Clear();
            if (meshFilter.sharedMesh != mesh)
                meshFilter.sharedMesh = mesh;
#if UNITY_2022
            var data = ToVertices(SourcePairs, Vector3.zero);
#else
            var data = ToVertices(SourcePairs, Vector3.zero);
#endif

            meshFilter.sharedMesh.vertices = data.vertices;
            meshFilter.sharedMesh.triangles = data.triangles;
            meshFilter.sharedMesh.uv = data.uvs;

            return data;
        }

        public static MeshData ToVertices(this IEnumerable<VertexEntry> source, Vector3 zeroPos)
        {
            MeshData meshData = new MeshData();
            List<Vector3> vertices = new();
            List<int> triangles = new();
            List<Vector2> uvs = new();
            int i = 0;
            int e = 0;

            foreach (var item in source)
            {
                e++;
            }

            foreach (var current in source)
            {
                if (current.Type == BuildNormalType.Default)
                {
                    Vector3 NormalVec = current.Normal.normalized * current.Size;
                    vertices.Add(current.Position + zeroPos);
                    vertices.Add(current.Position + zeroPos + NormalVec);
                }
                else
                {
                    Vector3 NormalVec = 0.5f * current.Size * current.Normal.normalized;
                    vertices.Add(current.Position + zeroPos - NormalVec);
                    vertices.Add(current.Position + zeroPos + NormalVec);
                }
                float hight = i / (float)e * 2;
                uvs.Add(new Vector2(0, hight));
                uvs.Add(new Vector2(1, hight));
                if (i > 0)
                {
                    int index0 = 2 * i - 2, index1 = 2 * i - 1, index2 = 2 * i, index3 = 1 + 2 * i;
                    triangles.Add(index1);
                    triangles.Add(index2);
                    triangles.Add(index0);
                    triangles.Add(index1);
                    triangles.Add(index2);
                    triangles.Add(index3);
                }
                i++;
            }

            meshData.vertices = vertices.ToArray();
            meshData.triangles = triangles.ToArray();
            meshData.uvs = uvs.ToArray();

            return meshData;
        }

        public static VertexEntry[] GenerateCurveMeshData(this ICustomCurveSource self, BuildNormalType normalType, Vector3 Normal, float headSize, float tailSize)
        {
            Vector3[] line = self.CreateCurve();
            VertexEntry[] vertexs = new VertexEntry[line.Length];
            for (int i = 0, e = line.Length; i < e; i++)
            {
                var current = vertexs[i] = new VertexEntry();
                current.Type = normalType;
                current.Position = line[i];
                current.Size = Mathf.Lerp(headSize, tailSize, (float)i / (float)e);
                current.Normal = Normal;
            }
            return vertexs;
        }

        public static VertexEntry[] GenerateCurveMeshData(this ICustomCurveSource self, BuildNormalType normalType, Vector3 Normal,EaseCurve size)
        {
            return GenerateCurveMeshData(self, normalType, Normal, size, 0, 1);
        }

        public static VertexEntry[] GenerateCurveMeshData(this ICustomCurveSource self, BuildNormalType normalType, Vector3 Normal, EaseCurve sizeCurve,float headSize,float tailSize)
        {
            Vector3[] line = self.CreateCurve();
            VertexEntry[] vertexs = new VertexEntry[line.Length];
            for (int i = 0, e = line.Length; i < e; i++)
            {
                var current = vertexs[i] = new VertexEntry();
                current.Type = normalType;
                current.Position = line[i];
                current.Size = Mathf.Lerp(headSize, tailSize, sizeCurve.Evaluate((float)i / (float)e));
                current.Normal = Normal;
            }
            return vertexs;
        }

        public static VertexEntry[] GenerateCurveMeshData(this ICustomCurveSource self, BuildNormalType normalType, Vector3 Normal, AnimationCurve size)
        {
            return GenerateCurveMeshData(self,normalType,Normal, size, 0, 1);   
        }

        public static VertexEntry[] GenerateCurveMeshData(this ICustomCurveSource self, BuildNormalType normalType, Vector3 Normal, AnimationCurve sizeCurve,float headSize,float tailSize)
        {
            Vector3[] line = self.CreateCurve();
            VertexEntry[] vertexs = new VertexEntry[line.Length];
            for (int i = 0, e = line.Length; i < e; i++)
            {
                var current = vertexs[i] = new VertexEntry();
                current.Type = normalType;
                current.Position = line[i];
                current.Size = Mathf.Lerp(headSize, tailSize, sizeCurve.Evaluate((float)i / (float)e));
                current.Normal = Normal;
            }
            return vertexs;
        }
    }
}
