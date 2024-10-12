using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Assimp;

namespace YJ.Map
{
    public class LoadMesh
    {
        public static IEnumerable<MeshInfo> LoadGLBMesh(string filePath)
        {
            var importer = new AssimpContext();
            var scene = importer.ImportFile(filePath);
            Logger.Debug($"Scene has {scene.MeshCount} meshes");
            var meshInfos = new List<MeshInfo>();
            scene.Meshes.ForEach(mesh =>
            {
                var verts = mesh.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z));
                var triangles = new List<int>();
                mesh.Faces.ForEach(face => triangles.AddRange(face.Indices));
                var normals = mesh.Normals.Select(n => new Vector3(n.X, n.Y, n.Z));
                meshInfos.Add(new MeshInfo(verts, triangles, normals, mesh.Name));
            });
            return meshInfos;
        }
    }
}