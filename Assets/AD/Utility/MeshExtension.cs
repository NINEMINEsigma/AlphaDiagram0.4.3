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

        public struct MeshData
        {
            public Vector3[] vertices;
            public int[] triangles;
        }

        public static void InitMesh(this MeshFilter meshFilter, IEnumerable<VertexEntry> SourcePairs)
        {
            InitMesh(meshFilter, new Mesh(), SourcePairs);
        }

        public static void InitMesh(this MeshFilter meshFilter, Mesh mesh, IEnumerable<VertexEntry> SourcePairs)
        {
            meshFilter.sharedMesh = mesh;

            var data = ToVertices(SourcePairs, meshFilter.transform.position);

            meshFilter.sharedMesh.vertices = data.vertices;
            meshFilter.sharedMesh.triangles = data.triangles;
        }

        public static MeshData ToVertices(this IEnumerable<VertexEntry> source, Vector3 zeroPos)
        {
            MeshData meshData = new MeshData();
            List<Vector3> vertices = new();
            List<int> triangles = new();
            int i = 0;

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
                    Vector3 NormalVec = current.Normal.normalized * current.Size * 0.5f;
                    vertices.Add(current.Position + zeroPos - NormalVec);
                    vertices.Add(current.Position + zeroPos + NormalVec);
                }
                if (i > 0)
                {
                    int it = 1 + 2 * i;
                    triangles.Add(it - 3);
                    triangles.Add(it - 2);
                    triangles.Add(it - 1);
                    triangles.Add(it - 2);
                    triangles.Add(it - 1);
                    triangles.Add(it);
                }
                i++;
            }

            meshData.vertices = vertices.ToArray();
            meshData.triangles = triangles.ToArray();

            return meshData;
        }

        public static VertexEntry[] GenerateCurveMeshData(this CustomCurveSource self, BuildNormalType normalType, Vector3 Normal,EaseCurve size)
        {
            Vector3[] line = CustomCurveSource.CreateCurve(self.allPoints, self.SEGMENT_COUNT, self.EaseCurve);
            VertexEntry[] vertexs=new VertexEntry[line.Length];
            for (int i = 0,e=line.Length; i <e ; i++)
            {
                var current= vertexs[i] = new VertexEntry();
                current.Type = normalType;
                current.Position = line[i];
                current.Size = size.Evaluate((float)i / (float)e);
                current.Normal = Normal;
            }
            return vertexs;
        }

        public static VertexEntry[] GenerateCurveMeshData(this CustomCurveSource self, BuildNormalType normalType, Vector3 Normal, AnimationCurve size)
        {
            Vector3[] line = CustomCurveSource.CreateCurve(self.allPoints, self.SEGMENT_COUNT, self.EaseCurve);
            VertexEntry[] vertexs = new VertexEntry[line.Length];
            for (int i = 0, e = line.Length; i < e; i++)
            {
                var current = vertexs[i] = new VertexEntry();
                current.Type = normalType;
                current.Position = line[i];
                current.Size = size.Evaluate((float)i / (float)e);
                current.Normal = Normal;
            }
            return vertexs;
        }
    }
}
