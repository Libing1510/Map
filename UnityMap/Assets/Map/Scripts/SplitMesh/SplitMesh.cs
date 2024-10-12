using System;
using System.Collections.Generic;
using System.Numerics;

namespace YJ.Map.SplitMesh
{
    public class SplitMesh
    {
        public YJRect uvRange { get; set; }
        public MeshInfo meshInfo { get; set; }

        public SplitMesh(MeshInfo mesh, YJRect uvRange)
        {
            meshInfo = mesh;
            this.uvRange = uvRange;
        }

        public MeshInfo[] Split(YJPlane plane, out MeshInfo cutInfo)
        {
            cutInfo = null;
            Vector3 point = plane.normal * -plane.distance;
            Vector3 normal = plane.normal.ToNormalized();
            MeshInfo a = new MeshInfo();
            MeshInfo b = new MeshInfo();
            Logger.Debug($"SplitMesh: vertices= {meshInfo.vertices.Count},{plane.distance},{point}");
            //
            bool[] above = new bool[meshInfo.vertices.Count];
            int[] newTriangles = new int[meshInfo.vertices.Count];
            for (int i = 0; i < newTriangles.Length; i++)
            {
                Vector3 vert = meshInfo.vertices[i];
                above[i] = Vector3.Dot(vert - point, normal) >= 0f;

                var uvT = meshInfo.uvs.Count > i ? meshInfo.uvs[i] : Vector2.Zero;
                var normalT = meshInfo.normals.Count > i ? meshInfo.normals[i] : Vector3.Zero;
                var tangentT = meshInfo.tangents.Count > i ? meshInfo.tangents[i] : Vector4.Zero;

                if (above[i])
                {
                    newTriangles[i] = a.vertices.Count;
                    a.Add(vert, uvT, normalT, tangentT);
                }
                else
                {
                    newTriangles[i] = b.vertices.Count;
                    b.Add(vert, uvT, normalT, tangentT);
                }
            }

            List<Vector3> cutPoint = new List<Vector3>();
            int triangleCount = meshInfo.triangles.Count / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int _i0 = meshInfo.triangles[i * 3];
                int _i1 = meshInfo.triangles[i * 3 + 1];
                int _i2 = meshInfo.triangles[i * 3 + 2];

                bool _a0 = above[_i0];
                bool _a1 = above[_i1];
                bool _a2 = above[_i2];
                if (_a0 && _a1 && _a2)
                {
                    a.triangles.Add(newTriangles[_i0]);
                    a.triangles.Add(newTriangles[_i1]);
                    a.triangles.Add(newTriangles[_i2]);
                }
                else if (!_a0 && !_a1 && !_a2)
                {
                    b.triangles.Add(newTriangles[_i0]);
                    b.triangles.Add(newTriangles[_i1]);
                    b.triangles.Add(newTriangles[_i2]);
                }
                else
                {
                    int up, down0, down1;
                    if (_a1 == _a2 && _a0 != _a1)
                    {
                        up = _i0;
                        down0 = _i1;
                        down1 = _i2;
                    }
                    else if (_a2 == _a0 && _a1 != _a2)
                    {
                        up = _i1;
                        down0 = _i2;
                        down1 = _i0;
                    }
                    else
                    {
                        up = _i2;
                        down0 = _i0;
                        down1 = _i1;
                    }
                    Vector3 pos0, pos1;
                    if (above[up])
                        SplitTriangle(a, b, point, normal, newTriangles, up, down0, down1, out pos0, out pos1);
                    else
                        SplitTriangle(b, a, point, normal, newTriangles, up, down0, down1, out pos1, out pos0);
                    cutPoint.Add(pos0);
                    cutPoint.Add(pos1);
                }
            }
            a.CombineVertices(0.001f);
            a.center = meshInfo.center;
            a.size = meshInfo.size;
            b.CombineVertices(0.001f);
            b.center = meshInfo.center;
            b.size = meshInfo.size;
            var result = new List<MeshInfo>
            {
                a,
                b
            };
            if (cutPoint.Count > 2)
            {
                cutInfo = FastFillCutEdges(cutPoint, point, normal);
            }
            Logger.Debug($"SplitMesh: h={point} above:{a.vertices.Count} blew={b.vertices.Count}");
            return result.ToArray();
        }

        private void SplitTriangle(MeshInfo top, MeshInfo bottom, Vector3 point, Vector3 normal, int[] newTriangles, int up, int down0, int down1, out Vector3 pos0, out Vector3 pos1)
        {
            Vector3 v0 = meshInfo.vertices[up];
            Vector3 v1 = meshInfo.vertices[down0];
            Vector3 v2 = meshInfo.vertices[down1];
            float topDot = Vector3.Dot(point - v0, normal);
            float aScale = Math.Clamp(topDot / Vector3.Dot(v1 - v0, normal), 0, 1);
            float bScale = Math.Clamp(topDot / Vector3.Dot(v2 - v0, normal), 0, 1);
            Vector3 pos_a = v0 + (v1 - v0) * aScale;
            Vector3 pos_b = v0 + (v2 - v0) * bScale;

            Vector2[] uvs = SplitUV();
            Vector3[] norms = SplitNormal();
            Vector4[] tangents = SplitTangent();

            int top_a = top.vertices.Count;
            top.Add(pos_a, uvs[0], norms[0], tangents[0]);
            int top_b = top.vertices.Count;
            top.Add(pos_b, uvs[1], norms[1], tangents[1]);
            top.triangles.Add(newTriangles[up]);
            top.triangles.Add(top_a);
            top.triangles.Add(top_b);

            int down_a = bottom.vertices.Count;
            bottom.Add(pos_a, uvs[0], norms[0], tangents[0]);
            int down_b = bottom.vertices.Count;
            bottom.Add(pos_b, uvs[1], norms[1], tangents[1]);

            bottom.triangles.Add(newTriangles[down0]);
            bottom.triangles.Add(newTriangles[down1]);
            bottom.triangles.Add(down_b);

            bottom.triangles.Add(newTriangles[down0]);
            bottom.triangles.Add(down_b);
            bottom.triangles.Add(down_a);

            pos0 = pos_a;
            pos1 = pos_b;

            Vector2[] SplitUV()
            {
                if (meshInfo.uvs == null || meshInfo.uvs.Count == 0)
                    return new Vector2[] { Vector2.Zero, Vector2.Zero };
                Vector2 u0 = meshInfo.uvs[up];
                Vector2 u1 = meshInfo.uvs[down0];
                Vector2 u2 = meshInfo.uvs[down1];

                Vector2 uv_a = (u0 + (u1 - u0) * aScale);
                Vector2 uv_b = ((u0 + (u2 - u0) * bScale));
                return new Vector2[] { uv_a, uv_b };
            }

            Vector3[] SplitNormal()
            {
                if (meshInfo.normals == null || meshInfo.normals.Count == 0)
                    return new Vector3[] { Vector3.Zero, Vector3.Zero };

                Vector3 n0 = meshInfo.normals[up];
                Vector3 n1 = meshInfo.normals[down0];
                Vector3 n2 = meshInfo.normals[down1];
                Vector3 normal_a = (n0 + (n1 - n0) * aScale).ToNormalized();
                Vector3 normal_b = (n0 + (n2 - n0) * bScale).ToNormalized();
                return new Vector3[] { normal_a, normal_b };
            }

            Vector4[] SplitTangent()
            {
                if (meshInfo.tangents == null || meshInfo.tangents.Count == 0)
                    return new Vector4[] { Vector4.Zero, Vector4.Zero };

                Vector4 t0 = meshInfo.tangents[up];
                Vector4 t1 = meshInfo.tangents[down0];
                Vector4 t2 = meshInfo.tangents[down1];
                Vector4 tangent_a = (t0 + (t1 - t0) * aScale).ToNormalized();
                Vector4 tangent_b = (t0 + (t2 - t0) * bScale).ToNormalized();
                tangent_a.W = t1.W;
                tangent_b.W = t2.W;
                return new Vector4[] { tangent_a, tangent_b };
            }
        }

        private MeshInfo FastFillCutEdges(List<Vector3> edges, Vector3 pos, Vector3 normal)
        {
            if (edges.Count < 3)
                throw new Exception("edges point less 3!");

            for (int i = 0; i < edges.Count - 3; i++)
            {
                Vector3 t = edges[i + 1];
                Vector3 temp = edges[i + 3];
                for (int j = i + 2; j < edges.Count - 1; j += 2)
                {
                    if ((edges[j] - t).LengthSquared() < 1e-6)
                    {
                        edges[j] = edges[i + 2];
                        edges[i + 3] = edges[j + 1];
                        edges[j + 1] = temp;
                        break;
                    }
                    if ((edges[j + 1] - t).LengthSquared() < 1e-6)
                    {
                        edges[j + 1] = edges[i + 2];
                        edges[i + 3] = edges[j];
                        edges[j] = temp;
                        break;
                    }
                }
                edges.RemoveAt(i + 2);
            }
            edges.RemoveAt(edges.Count - 1);

            Vector4 tangent = meshInfo.CalculateTangent(normal);

            MeshInfo cutEdges = new MeshInfo();
            for (int i = 0; i < edges.Count; i++)
                cutEdges.Add(edges[i], Vector2.Zero, normal, tangent);
            int count = edges.Count - 1;
            for (int i = 1; i < count; i++)
            {
                cutEdges.triangles.Add(0);
                cutEdges.triangles.Add(i);
                cutEdges.triangles.Add(i + 1);
            }

            cutEdges.center = meshInfo.center;
            cutEdges.size = meshInfo.size;
            cutEdges.MapperCube(uvRange);
            return cutEdges;
        }
    }
}