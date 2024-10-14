using System.Collections.Generic;
using System.Linq;
using VoronatorSharp;
using YJ.Map.Clip;
using Vector3 = System.Numerics.Vector3;

namespace YJ.Map.DelaunatorVoronator
{
    public class DelaunatorVoronatorTools
    {
        public static IEnumerable<MeshInfo> PoygonToMesh(Polygon polygon)
        {
            var meshInfos = new List<MeshInfo>();
            polygon.polygons.ForEach(p =>
            {
                var points = p.Select(point => point.ToVSVector2()).ToList();
                var delaunator = new Delaunator(points);
                var triangles = delaunator.GetTriangles().ToList();

                List<Vector3> vertices = new List<Vector3>();
                triangles.ForEach(t =>
                {
                    vertices.Add(t.Point1.ToSnVector2().ToSnVector3());
                    vertices.Add(t.Point2.ToSnVector2().ToSnVector3());
                    vertices.Add(t.Point3.ToSnVector2().ToSnVector3());
                });
                var triang = Enumerable.Range(0, vertices.Count);
                var normals = Enumerable.Repeat(Vector3.UnitY, vertices.Count);
                var mi = new MeshInfo(vertices, triang, normals);
                mi.VertexRemovalRepeat();
                meshInfos.Add(mi);
            });
            return meshInfos;
        }

        public static IEnumerable<Line2D> PolygonRoad(Polygon ground, Polygon wall)
        {
            var points = new List<Vector2>();
            ground.polygons.ForEach(p => points.AddRange(p.Where(t => t != null).Select(t => t.ToVSVector2())));
            wall.polygons.ForEach(p => points.AddRange(p.Where(t => t != null).Select(t => t.ToVSVector2())));
            var voronator = new Voronator(points);
            List<Line2D> roadLines = new List<Line2D>();
            for (int i = 0; i < voronator.Inedges.Length; i++)
            {
                var polygon = voronator.GetPolygon(i);
                if (polygon == null) continue;
                var valids = new List<bool>();
                //点在地面不在墙面
                polygon.ForEach(p => valids.Add(ground.IsPointInsdie(p.ToSnVector2())
                                    && !wall.IsPointInsdie(p.ToSnVector2())));
                for (int j = 1; j < polygon.Count; j++)
                {
                    if (valids[j - 1] && valids[j])
                        roadLines.Add(new Line2D(polygon[j - 1].ToSnVector2(), polygon[j].ToSnVector2()));
                }
                //首尾相连
                if (valids[0] && valids[^1])
                    roadLines.Add(new Line2D(polygon[0].ToSnVector2(), polygon[^1].ToSnVector2()));
            }
            return roadLines;
        }
    }
}