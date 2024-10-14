using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using YJ.Map.Clip;
using YJ.Map.ConvexHull;
using YJ.Map.SplitMesh;

namespace YJ.Map
{
    public class Mesh2Map
    {
        private enum SplitState
        {
            SplitCeiling,
            SplitGround,
        }

        private List<MeshInfo> m_BelowMeshInfos;
        private List<MeshInfo> m_AboveMeshInfos;
        private float m_GroundHigh = 0.1f;
        private float m_WallHigh = 1.0f;
        private float m_WallExtend = 0.1f;
        private SplitState m_SplitState = SplitState.SplitCeiling;
        private Action<int, List<Vector3>, string> m_OnWallComplete;
        private Action<int, List<Vector3>, string> m_OnGroundComplete;

        private List<Polygon> m_WallPolygons;
        private List<Polygon> m_GroundPolygons;
        public Polygon WallPolygon { get; private set; }
        public Polygon GroundPolygon { get; private set; }

        public List<Line2D> RoadEdge { get; private set; }

        public Mesh2Map(float groudhHigh, float wallHigh, float wallExtend)
        {
            m_GroundHigh = groudhHigh;
            m_WallHigh = wallHigh;
            m_WallExtend = wallExtend;
            m_BelowMeshInfos = new List<MeshInfo>();
            m_AboveMeshInfos = new List<MeshInfo>();
            m_WallPolygons = new List<Polygon>();
            m_GroundPolygons = new List<Polygon>();
            RoadEdge = new List<Line2D>();
        }

        public void ThreadSplit(IEnumerable<MeshInfo> meshInfos, Action<int, List<Vector3>, string> onWallComplete, Action<int, List<Vector3>, string> onGroundComplete, Action<List<MeshInfo>> onGround = null, Action<List<MeshInfo>> onWall = null)
        {
            m_OnWallComplete = onWallComplete;
            m_OnGroundComplete = onGroundComplete;

            Logger.Debug($"ThreadSplit: wallH={m_WallHigh},groundH={m_GroundHigh},{Vector3.UnitY.X}/{Vector3.UnitY.Y}/{Vector3.UnitY.Z}");
            //先裁剪天花板
            var ceilingPlane = new YJPlane(Vector3.UnitY, Vector3.UnitY * m_WallHigh);
            var ceilingTread = new SplitMeshThreadPool(ceilingPlane, new YJRect(0, 0, 1, 1), meshInfos);
            ceilingTread.ThreadDoSplitMesh(false, false, OnSPplitComplete);

            // 分离墙面和地面,天花板舍弃
            var wallAndGroundMeshes = new List<MeshInfo>();
            wallAndGroundMeshes.AddRange(m_BelowMeshInfos);
            Logger.Debug($"ThreadSplit: ceiling above={m_AboveMeshInfos.Count},below={m_BelowMeshInfos.Count}");
            m_AboveMeshInfos.Clear();
            m_BelowMeshInfos.Clear();
            var groundPlane = new YJPlane(Vector3.UnitY, Vector3.UnitY * m_GroundHigh);
            var groundTread = new SplitMeshThreadPool(groundPlane, new YJRect(0, 0, 1, 1), wallAndGroundMeshes);
            groundTread.ThreadDoSplitMesh(true, true, OnSPplitComplete);

            Logger.Debug($"ThreadSplit: wall above={m_AboveMeshInfos.Count},below={m_BelowMeshInfos.Count}");

            onGround?.Invoke(m_BelowMeshInfos);
            onWall?.Invoke(m_AboveMeshInfos);

            //裁剪完简化到2D平面凸包
            ConvexHullMesh();

            void OnSPplitComplete(int id, List<MeshInfo> aboveMeshes, List<MeshInfo> belowMeshes, MeshInfo cut)
            {
                m_AboveMeshInfos.AddRange(aboveMeshes);
                m_BelowMeshInfos.AddRange(belowMeshes);
            }
        }

        /// <summary>
        /// 将地面和墙面投影到凸包多边形。
        /// </summary>
        private void ConvexHullMesh()
        {
            // 墙面处理
            MeshConvexHullThreadPool wallConvexHull = new MeshConvexHullThreadPool();
            wallConvexHull.ThreadDoConvexHullMesh(m_AboveMeshInfos, m_WallExtend, OnConvexHullWallComplete);
            //地面处理
            MeshConvexHullThreadPool groundConvexHull = new MeshConvexHullThreadPool();
            wallConvexHull.ThreadDoConvexHullMesh(m_BelowMeshInfos, 0, OnConvexGroundComplete);
            //释放无用数组
            m_AboveMeshInfos.Clear();
            m_BelowMeshInfos.Clear();
            Voronator();
            void OnConvexHullWallComplete(int id, List<Vector3> vertices, string name)
            {
                var vector2ds = vertices.Select(v => v.ToSnVector2());
                var polygon = new Polygon(vector2ds);
                if (polygon != null)
                    m_WallPolygons.Add(polygon);
                m_OnWallComplete?.Invoke(id, vertices, name);
            }
            void OnConvexGroundComplete(int id, List<Vector3> vertices, string name)
            {
                var vector2ds = vertices.Select(v => v.ToSnVector2());
                var polygon = new Polygon(vector2ds);
                if (polygon != null)
                    m_GroundPolygons.Add(polygon);
                m_OnGroundComplete?.Invoke(id, vertices, name);
            }
        }

        private void Voronator()
        {
            Logger.Debug($"Voronator: wall={m_WallPolygons.Count},ground ={m_GroundPolygons.Count}");
            var wallPolygon = m_WallPolygons.FirstOrDefault(p => p.polygons != null);
            m_WallPolygons.ForEach(wp => wallPolygon = wallPolygon.Union(wp, Clipper2Lib.FillRule.EvenOdd));
            var groundPolygon = m_GroundPolygons.FirstOrDefault(p => p.polygons != null);
            m_GroundPolygons.ForEach(gp => groundPolygon = groundPolygon.Union(gp, Clipper2Lib.FillRule.EvenOdd));

            WallPolygon = wallPolygon;
            GroundPolygon = groundPolygon;
            RoadEdge.Clear();
            if (groundPolygon == null || wallPolygon == null)
            {
                Logger.Error("Road Polygon cant be null!");
                return;
            }

            var lines = DelaunatorVoronator.DelaunatorVoronatorTools.PolygonRoad(groundPolygon, wallPolygon);
            Logger.Debug($"Voronator: wall={m_WallPolygons.Count},ground ={m_GroundPolygons.Count} lines ={lines.Count()}");

            RoadEdge.AddRange(lines);
        }
    }
}