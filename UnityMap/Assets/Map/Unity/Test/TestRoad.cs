using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YJ.Map.Dijkstra;

namespace YJ.Unity
{
    public class TestRoad : MonoBehaviour
    {
        private List<System.Numerics.Vector3> points = new List<System.Numerics.Vector3>();
        private List<Edge> edges = new List<Edge>();
        private List<Node> nodes = new List<Node>();
        private DijkstraRouter dijkstra;
        public int count = 10;
        public int start = 0;
        public int end = 7;
        private List<Edge> roads = new List<Edge>();

        private void Start()
        {
            //Init();
            ReadLocalMap($"{Application.dataPath}/road.txt");
            dijkstra = new DijkstraRouter();
            dijkstra.Initialize(edges, nodes);
            count = nodes.Count;
        }

        public void ReadLocalMap(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"cant find {filePath}");
                return;
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var strs = line.Split(new char[] { ',' });
                var a = new System.Numerics.Vector3(float.Parse(strs[0]), 0.0f, float.Parse(strs[1]));
                var b = new System.Numerics.Vector3(float.Parse(strs[2]), 0.0f, float.Parse(strs[3]));
                var aIndex = points.FindIndex(p => p == a);
                if (aIndex == -1) { points.Add(a); aIndex = points.Count - 1; }
                var bIndex = points.FindIndex(p => p == b);
                if (bIndex == -1) { points.Add(b); bIndex = points.Count - 1; }
                var aNode = new Node(aIndex, a);
                var bNode = new Node(bIndex, b);
                if (nodes.FirstOrDefault(n => n.id == aIndex) == null)
                { nodes.Add(aNode); }

                if (nodes.FirstOrDefault(n => n.id == bIndex) == null)
                { nodes.Add(bNode); }

                edges.Add(new Edge(aNode, bNode, System.Numerics.Vector3.Distance(a, b)));
                //if (points.Count > 100) break;
            }
            Debug.Log($"read local {filePath} end");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var sNode = nodes[start];
                var dNode = nodes[end];
                var result = dijkstra.GetRoute(sNode, dNode);
                if (result != null) roads = result;
                Debug.Log($"roads:{roads.Count}");
            }
            start = Math.Clamp(start, 0, count - 1);
            end = Math.Clamp(end, 0, count - 1);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            points.ForEach(p =>
            {
                Gizmos.DrawSphere(ToUnity(p), 0.1f);
            });
            Gizmos.color = Color.white;
            edges.ForEach(e =>
            {
                Gizmos.DrawLine(ToUnity(e.aNode.vector), ToUnity(e.bNode.vector));
            });
            Gizmos.color = Color.green;
            roads.ForEach(p =>
            {
                Gizmos.DrawLine(ToUnity(p.aNode.vector), ToUnity(p.bNode.vector));
            });
            if (points.Count == 0) return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(ToUnity(nodes[start].vector), 0.3f);
            Gizmos.DrawSphere(ToUnity(nodes[end].vector), 0.3f);
        }

        public static Vector3 ToUnity(System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}