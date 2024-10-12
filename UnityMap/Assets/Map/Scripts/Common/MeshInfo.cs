using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.Plastic.Newtonsoft.Json;

namespace YJ.Map
{
    public class MeshInfo
    {
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector3> normals;
        public List<Vector2> uvs;
        public List<Vector4> tangents;
        public Vector3 size;
        public Vector3 center;
        public string name;

        public MeshInfo() : this(new Vector3[0], new int[0], new Vector3[0])
        {
        }

        public MeshInfo(IEnumerable<Vector3> vertices, IEnumerable<int> triangles,
            IEnumerable<Vector3> normals, string name = "null")
            : this(vertices, triangles, normals, new Vector2[0],
                  new Vector4[0], Vector3.Zero, Vector3.Zero)
        {
        }

        public MeshInfo(IEnumerable<Vector3> vertices, IEnumerable<int> triangles,
                         IEnumerable<Vector3> normals, IEnumerable<Vector2> uvs,
                        IEnumerable<Vector4> tangents, Vector3 size, Vector3 center)
        {
            this.vertices = vertices.ToList();
            this.triangles = triangles.ToList();
            this.uvs = uvs.ToList();
            this.normals = normals.ToList();
            this.tangents = tangents.ToList();
            this.size = size;
            this.center = center;
        }

        public class Temp
        {
            public List<UnityEngine.Vector3> vector3s;
            public List<int> trian = new List<int>();
            public string name;
        }

        public MeshInfo(string json)
        {
            var temp = UnityEngine.JsonUtility.FromJson<Temp>(json);
            this.vertices = temp.vector3s.Select(v => new Vector3(v.x, v.y, v.z)).ToList();
            this.triangles = temp.trian;
            this.uvs = new List<Vector2>();
            this.normals = new List<Vector3>();
            this.tangents = new List<Vector4>();
            this.size = Vector3.Zero;
            this.center = Vector3.Zero;
        }

        public string ToJson()
        {
            return UnityEngine.JsonUtility.ToJson(new Temp() { vector3s = vertices.Select(v => new UnityEngine.Vector3(v.X, v.Y, v.Z)).ToList(), trian = this.triangles, name = this.name });
        }

        /// <summary>
        /// 顶点去除重复
        /// </summary>
        public void VertexRemovalRepeat()
        {
            var vert = new List<Vector3>();
            var trian = new List<int>(triangles);
            var uv = new List<Vector2>();
            var norm = new List<Vector3>();
            var tang = new List<Vector4>();

            for (int i = 0; i < vertices.Count(); i++)
            {
                int index = vert.IndexOf(vertices.ElementAt(i));
                if (index >= 0)
                {
                    for (int j = 0; j < trian.Count; j++)
                    {
                        if (trian[j] == i) trian[j] = index;
                    }
                }
                else
                {
                    vert.Add(vertices.ElementAt(i));

                    if (uvs.Count > i)
                        uv.Add(uvs.ElementAt(i));
                    if (norm.Count > i)
                        norm.Add(normals.ElementAt(i));
                    if (tang.Count > i)
                        tang.Add(tangents.ElementAt(i));
                    for (int j = 0; j < trian.Count; j++)
                    {
                        if (trian[j] == i) trian[j] = vert.Count - 1;
                    }
                }
            }

            vertices = vert;
            triangles = trian;
            uvs = uv;
            normals = norm;
            tangents = tang;
        }

        public void ResetSizeAndCenter()
        {
            var sumCenter = Vector3.Zero;
            var minSize = Vector3.One * float.MaxValue;
            var maxSize = Vector3.One * float.MinValue;

            foreach (var v in vertices)
            {
                sumCenter += v;
                if (v.X < minSize.X) minSize.X = v.X;
                if (v.Y < minSize.Y) minSize.Y = v.Y;
                if (v.Z < minSize.Z) minSize.Z = v.Z;
                if (v.X > maxSize.X) maxSize.X = v.X;
                if (v.Y > maxSize.Y) maxSize.Y = v.Y;
                if (v.Z > maxSize.Z) maxSize.Z = v.Z;
            }

            center = sumCenter / vertices.Count();
            size = maxSize - minSize + Vector3.One * 0.35f;

            for (int i = 0; i < vertices.Count(); i++)
            {
                vertices[i] -= center;
            }
        }

        public void Add(Vector3 vert, Vector3 normal)
        {
            Add(vert, Vector2.Zero, normal, Vector4.Zero);
        }

        public void Add(Vector3 vert, Vector2 uv, Vector3 normal, Vector4 tangent)
        {
            vertices.Add(vert);
            if (uv != Vector2.Zero)
                uvs.Add(uv);
            if (normal != Vector3.Zero)
                normals.Add(normal);
            if (tangent != Vector4.Zero)
                tangents.Add(tangent);
        }

        /// <summary>
        /// UV 数据整合
        /// </summary>
        /// <param name="range"></param>
        public void MapperCube(YJRect range)
        {
            if (uvs.Count == 0) return;

            if (uvs.Count < vertices.Count)
                uvs = new List<Vector2>(vertices.Count);
            int count = triangles.Count / 3;
            for (int i = 0; i < count; i++)
            {
                int _i0 = triangles[i * 3];
                int _i1 = triangles[i * 3 + 1];
                int _i2 = triangles[i * 3 + 2];

                Vector3 v0 = vertices[_i0] - center + size / 2f;
                Vector3 v1 = vertices[_i1] - center + size / 2f;
                Vector3 v2 = vertices[_i2] - center + size / 2f;
                v0 = new Vector3(v0.X / size.X, v0.Y / size.Y, v0.Z / size.Z);
                v1 = new Vector3(v1.X / size.X, v1.Y / size.Y, v1.Z / size.Z);
                v2 = new Vector3(v2.X / size.X, v2.Y / size.Y, v2.Z / size.Z);

                Vector3 a = v0 - v1;
                Vector3 b = v2 - v1;
                Vector3 dir = Vector3.Cross(a, b);
                float x = Math.Abs(Vector3.Dot(dir, Vector3.UnitX));
                float y = Math.Abs(Vector3.Dot(dir, Vector3.UnitY));
                float z = Math.Abs(Vector3.Dot(dir, Vector3.UnitZ));
                if (x > y && x > z)
                {
                    uvs[_i0] = new Vector2(v0.Z, v0.Y);
                    uvs[_i1] = new Vector2(v1.Z, v1.Y);
                    uvs[_i2] = new Vector2(v2.Z, v2.Y);
                }
                else if (y > x && y > z)
                {
                    uvs[_i0] = new Vector2(v0.X, v0.Z);
                    uvs[_i1] = new Vector2(v1.X, v1.Z);
                    uvs[_i2] = new Vector2(v2.X, v2.Z);
                }
                else if (z > x && z > y)
                {
                    uvs[_i0] = new Vector2(v0.X, v0.Y);
                    uvs[_i1] = new Vector2(v1.X, v1.Y);
                    uvs[_i2] = new Vector2(v2.X, v2.Y);
                }

                uvs[_i0] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i0].X, range.yMin + (range.yMax - range.yMin) * uvs[_i0].Y);
                uvs[_i1] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i1].X, range.yMin + (range.yMax - range.yMin) * uvs[_i1].Y);
                uvs[_i2] = new Vector2(range.xMin + (range.xMax - range.xMin) * uvs[_i2].X, range.yMin + (range.yMax - range.yMin) * uvs[_i2].Y);
            }
        }

        /// <summary>
        ///顶点闭合
        /// </summary>
        /// <param name="range"></param>
        public void CombineVertices(float range)
        {
            range *= range;
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    bool dis = (vertices[i] - vertices[j]).LengthSquared() < range;
                    bool uv = uvs.Count > j ? (uvs[i] - uvs[j]).LengthSquared() < range : true;

                    bool dir = normals.Count > j ? Vector3.Dot(normals[i], normals[j]) > 0.999f : true;
                    if (dis && uv && dir)
                    {
                        for (int k = 0; k < triangles.Count; k++)
                        {
                            if (triangles[k] == j)
                                triangles[k] = i;
                            if (triangles[k] > j)
                                triangles[k]--;
                        }
                        vertices.RemoveAt(j);
                        if (normals.Count > j)
                            normals.RemoveAt(j);
                        if (tangents.Count > j)
                            tangents.RemoveAt(j);
                        if (uvs.Count > j)
                            uvs.RemoveAt(j);
                    }
                }
            }
        }

        /// <summary>
        /// 三角反向
        /// </summary>
        public void Reverse()
        {
            int count = triangles.Count / 3;
            for (int i = 0; i < count; i++)
            {
                int t = triangles[i * 3 + 2];
                triangles[i * 3 + 2] = triangles[i * 3 + 1];
                triangles[i * 3 + 1] = t;
            }
            count = vertices.Count;
            for (int i = 0; i < count; i++)
            {
                if (normals.Count > i)
                    normals[i] *= -1;
                if (tangents.Count <= i)
                    continue;
                Vector4 tan = tangents[i];
                tan.W = -1;
                tangents[i] = tan;
            }
        }

        public Vector4 CalculateTangent(Vector3 normal)
        {
            Vector3 tan = Vector3.Cross(normal, Vector3.UnitY);
            if (tan == Vector3.Zero)
                tan = Vector3.Cross(normal, Vector3.UnitZ);
            tan = Vector3.Cross(tan, normal);
            return new Vector4(tan.X, tan.Y, tan.Z, 1.0f);
        }
    }
}