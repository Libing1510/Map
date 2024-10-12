using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace YJ.Unity
{
    public class ShowPolygon : MonoBehaviour
    {
        public string file;
        public Color pointColor = Color.yellow;
        public Color lineColor = Color.white;

        private List<List<Vector3>> m_Points;

        private void Start()
        {
            m_Points = new List<List<Vector3>>();
            var lines = File.ReadAllLines($"{Application.dataPath}/{file}");
            lines.ToList().ForEach(line =>
            {
                var vects = new List<Vector3>();
                var vectStrs = line.Split('!');
                vectStrs.ToList().ForEach(vect =>
                {
                    var strs = vect.Split(',');
                    vects.Add(new Vector3(float.Parse(strs[0]), 0, float.Parse(strs[1])));
                });
                m_Points.Add(vects);
            });
        }

        private void OnDrawGizmos()
        {
            if (m_Points == null) return;
            m_Points.ForEach(p =>
            {
                Gizmos.color = pointColor;
                p.ForEach(v => Gizmos.DrawSphere(v, 0.1f));
                Gizmos.color = lineColor;
                Gizmos.DrawLineStrip(p.ToArray(), true);
            });
        }
    }
}