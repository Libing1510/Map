using OuelletConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace YJ.Map.ConvexHull
{
    public class MeshConvexHull
    {
        private List<Vector3> m_Vertices;

        public MeshConvexHull(List<Vector3> vectors)
        {
            m_Vertices = vectors;
        }

        public List<Vector3> ConvexHullMesh()
        {
            var vertices = new double[m_Vertices.Count][];
            for (int i = 0; i < m_Vertices.Count; i++)
            {
                vertices[i] = new double[] { m_Vertices[i].X, m_Vertices[i].Y, m_Vertices[i].Z };
            }
            var convexHull = MIConvexHull.ConvexHull.Create(vertices);
            double[][] hullPoints = convexHull.Result.Points.Select(p => p.Position).ToArray();
            List<Vector3> hullVertices = new List<Vector3>();
            for (int i = 0; i < hullPoints.Length; i++)
            {
                hullVertices.Add(new Vector3((float)hullPoints[i][0], (float)hullPoints[i][1], (float)hullPoints[i][2]));
            }
            return hullVertices;
        }

        public List<Vector3> ConvexHullMesh2D(float extend)
        {
            var now = DateTime.Now;
            var windowsPoints = m_Vertices.Select(p => new Point(p.X, p.Z)).ToList();

            var ouelletConvexHull = new OuelletConvexHull.ConvexHull(windowsPoints);
            ouelletConvexHull.CalcConvexHull(ConvexHullThreadUsage.OnlyOne);
            var ouelletAsVertices = ouelletConvexHull.GetResultsAsArrayOfPoint()
            .Select(p => new Vector3((float)p.X, 0f, (float)p.Y)).ToList();

            var interval = DateTime.Now - now;
            Logger.Debug($"ConvexHullMesh  v={m_Vertices.Count} to {ouelletAsVertices.Count} using {DateTime.Now - now}");

            for (int i = 1; i < ouelletAsVertices.Count; i++)
            {
                if (Vector3.Distance(ouelletAsVertices[i - 1], ouelletAsVertices[i]) < 0.1f)
                {
                    ouelletAsVertices[i - 1] = (ouelletAsVertices[i] + ouelletAsVertices[i - 1]) * 0.5f;
                    ouelletAsVertices.RemoveAt(i);
                    i--;
                }
            }

            return extend > 0 ? ExpandVertices(ouelletAsVertices.ToArray(), extend).ToList() : ouelletAsVertices;
        }

        private Vector3[] ExpandVertices(Vector3[] vertices, float factor)
        {
            Vector3 centroid = CalculateCentroid(vertices); // 计算重心
            Vector3[] scaledVertices = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                // 计算当前顶点相对于重心的方向向量
                Vector3 direction = (vertices[i] - centroid).ToNormalized();

                // 按照给定的倍数缩放
                scaledVertices[i] = vertices[i] + direction * factor;
            }

            return scaledVertices;
        }

        private Vector3 CalculateCentroid(Vector3[] vertices)
        {
            Vector3 centroid = Vector3.Zero;

            foreach (var vertex in vertices)
            {
                centroid += vertex;
            }
            return centroid / vertices.Length; // 返回重心
        }
    }
}