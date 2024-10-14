using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YJ.Map.Dijkstra;
using YJ.Unity.Extension;

namespace YJ.Unity
{
    public class TestRoad : MonoBehaviour
    {
        private List<Edge> m_Edges = new List<Edge>();
        private List<Node> m_Nodes = new List<Node>();
        private YJMap m_Map;
        public int count = 10;
        public int start = 0;
        public int end = 7;
        [SerializeField]
        private float m_Disttance;
        public bool buildRoute;
        private List<Edge> m_Roads = new List<Edge>();
        public float boxSize;
        private void Start()
        {
            ReadLocalMap($"{Application.dataPath}/road.txt");
        }

        private void ReadLocalMap(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"cant find {filePath}");
                return;
            }
            m_Map = new YJMap();
            m_Map.ReadLocalMap(filePath);
            m_Edges = m_Map.Edges;
            m_Nodes = m_Map.Nodes;
            count = m_Nodes.Count;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GetRoute();
            }
        }

        private void GetRoute()
        {
            var sNode = m_Nodes[start];
            var dNode = m_Nodes[end];
            var result = m_Map.GetRoute(sNode, dNode);
            if (result != null) m_Roads = result.ToList();
            Debug.Log($"roads:{m_Roads.Count},disntacne={m_Disttance}");
        }

        public void SetMap(IEnumerable<Node> nodes, IEnumerable<Edge> edges)
        {
            m_Map = new YJMap(nodes, edges);
            m_Edges = m_Map.Edges;
            m_Nodes = m_Map.Nodes;
            count = m_Nodes.Count;
            start = Math.Clamp(start, 0, count - 1);
            end = Math.Clamp(end, 0, count - 1);
        }

        private void OnDrawGizmos()
        {
            start = Math.Clamp(start, 0, count - 1);
            end = Math.Clamp(end, 0, count - 1);
            if (buildRoute)
            {
                GetRoute();
                buildRoute = false;
            }

            Gizmos.color = Color.blue;
            m_Nodes.ForEach(p =>
            {
                Gizmos.DrawSphere(p.vector.ToUVector3(), 0.1f);
                Gizmos.DrawWireCube(p.vector.ToUVector3(),Vector3.one*boxSize);
            });
            Gizmos.color = Color.white;
            m_Edges.ForEach(e =>
            {
                Gizmos.DrawLine(e.aNode.vector.ToUVector3(), e.bNode.vector.ToUVector3());
            });
            Gizmos.color = Color.green;
            m_Roads.ForEach(p =>
            {
                Gizmos.DrawLine(p.aNode.vector.ToUVector3(), p.bNode.vector.ToUVector3());
            });
            if (m_Nodes.Count == 0) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Nodes[start].vector.ToUVector3(), 0.15f);
            Gizmos.DrawSphere(m_Nodes[end].vector.ToUVector3(), 0.15f);
            m_Disttance = System.Numerics.Vector3.Distance(m_Nodes[start].vector, m_Nodes[end].vector);
        }
    }
}