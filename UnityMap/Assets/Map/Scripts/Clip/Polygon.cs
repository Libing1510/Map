using Clipper2Lib;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace YJ.Map.Clip
{
    public class Polygon
    {
        private PathsD m_Polygons;
        internal PathsD polygons => m_Polygons;

        public List<List<Vector2>> GetVectors()
        {
            List<List<Vector2>> vectors = new List<List<Vector2>>();
            m_Polygons.ForEach(pds =>
            {
                var vects = new List<Vector2>();
                pds.ForEach(pd => vects.Add(pd.ToSNVector2()));
                vectors.Add(vects);
            });
            return vectors;
        }

        public Polygon(IEnumerable<Vector2> vectors)
        {
            var doubleList = new List<double>();
            vectors.ToList().ForEach(v => doubleList.AddRange(new double[] { v.X, v.Y }));
            m_Polygons = new PathsD() { Clipper.MakePath(doubleList.ToArray()) };
        }

        internal Polygon(PathsD polygon)
        {
            m_Polygons = polygon;
        }

        /// <summary>
        /// 求两者的相交部分
        /// </summary>
        /// <param name="clip">裁剪范围</param>
        /// <param name="rule">
        /// 填充规则：
        /// Even-Odd: 奇数次环绕的部分被填充，偶数此次不填充
        /// Non-Zero: 所有的非零环绕次数部分被填充
        /// Positive: 所有环绕次数大于零的部分被填充
        /// Negative: 所有环绕次数小于零的被填充
        /// </param>
        /// <returns></returns>
        public Polygon Intersection(Polygon clip, FillRule rule)
        {
            var pd = Clipper.Intersect(m_Polygons, clip.polygons, rule);
            return new Polygon(pd);
        }

        /// <summary>
        /// 求两者的并集部分
        /// </summary>
        /// <param name="clip">裁剪范围</param>
        /// <param name="rule">
        /// 填充规则：
        /// Even-Odd: 奇数次环绕的部分被填充，偶数此次不填充
        /// Non-Zero: 所有的非零环绕次数部分被填充
        /// Positive: 所有环绕次数大于零的部分被填充
        /// Negative: 所有环绕次数小于零的被填充
        /// <returns></returns>
        public Polygon Union(Polygon clip, FillRule rule)
        {
            if (clip == null || clip.polygons == null) return this;
            var pd = Clipper.Union(m_Polygons, clip.polygons, rule);
            return new Polygon(pd);
        }

        /// <summary>
        /// 获取Clip区域以外的区域
        /// </summary>
        /// <param name="clip">裁剪范围</param>
        /// <param name="rule">
        /// 填充规则：
        /// Even-Odd: 奇数次环绕的部分被填充，偶数此次不填充
        /// Non-Zero: 所有的非零环绕次数部分被填充
        /// Positive: 所有环绕次数大于零的部分被填充
        /// Negative: 所有环绕次数小于零的被填充
        /// <returns></returns>
        public Polygon Difference(Polygon clip, FillRule rule)
        {
            var pd = Clipper.Difference(m_Polygons, clip.polygons, rule);
            return new Polygon(pd);
        }

        /// <summary>
        /// 获取两个区域互不重复的区域
        /// </summary>
        /// <param name="clip">裁剪范围</param>
        /// <param name="rule">
        /// 填充规则：
        /// Even-Odd: 奇数次环绕的部分被填充，偶数此次不填充
        /// Non-Zero: 所有的非零环绕次数部分被填充
        /// Positive: 所有环绕次数大于零的部分被填充
        /// Negative: 所有环绕次数小于零的被填充
        /// <returns></returns>
        public Polygon XOR(Polygon clip, FillRule rule)
        {
            var pd = Clipper.Xor(m_Polygons, clip.polygons, rule);
            return new Polygon(pd);
        }

        /// <summary>
        /// 点在区域内
        /// </summary>
        /// <param name="vector">点位置</param>
        /// <returns></returns>
        public bool IsPointInsdie(Vector2 vector)
        {
            var point = new PointD(vector.X, vector.Y);
            bool result = false;
            for (int i = 0; i < m_Polygons.Count; i++)
            {
                if (Clipper.PointInPolygon(point, m_Polygons[i]) != PointInPolygonResult.IsOutside)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}