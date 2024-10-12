using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace YJ.Map.Dijkstra
{
    public class DijkstraRouter
    {
        private float[,] m_Graph;
        private List<Edge> m_Edges;
        private List<Node> m_Nodes;
        private List<NodeItem> m_NodeItems;
        private bool m_IsInited = false;

        public DijkstraRouter()
        {
            m_Edges = new List<Edge>();
            m_Nodes = new List<Node>();
            m_NodeItems = new List<NodeItem>();
        }

        /// <summary>
        /// 遍历所有点，计算出最短路径
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="nodes"></param>
        public void Initialize(IEnumerable<Edge> edges, IEnumerable<Node> nodes)
        {
            m_Edges = edges.ToList();
            m_Nodes = nodes.ToList();
            m_NodeItems.Clear();
            int length = nodes.Count();
            m_Graph = new float[length, length];
            // 二维数组遍历
            var range = Enumerable.Range(0, length);
            foreach (var row in range)
            {
                var rowNode = m_Nodes[row];
                foreach (var col in range)
                {
                    if (row == col)
                    {
                        m_Graph[row, col] = 0;
                        continue;
                    }
                    var colNode = m_Nodes[col];
                    var edge = GetFirstOrDefaultEdge(rowNode.id, colNode.id);
                    m_Graph[row, col] = edge == null ? float.MaxValue : edge.weigth;
                }
                m_NodeItems.Add(new NodeItem(row, rowNode));
            }
        }

        /// <summary>
        /// 计算出最短路径
        /// </summary>
        /// <param name="statNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public List<Edge> GetRoute(Node statNode, Node endNode)
        {
            Node sNode = null;
            Node dNode = null;

            try
            {
                sNode = m_Nodes.FirstOrDefault(n => n.id == statNode.id);
                dNode = m_Nodes.FirstOrDefault(n => n.id == endNode.id);
                if (sNode == null || dNode == null)
                    throw new ArgumentNullException("Cant found target node in map");
                //标记起点为算法原点
                m_NodeItems.FirstOrDefault(n => n.node.id == dNode.id).used = true;
                //对应行到底各个顶点的距离
                m_NodeItems.ForEach(n =>
                {
                    n.weight = GetRowArray(m_Nodes.IndexOf(sNode))[n.index];
                    n.roadNodes.Add(sNode);
                });

                //所有路径遍历一边
                while (m_NodeItems.Any(n => !n.used))
                {
                    var item = GetUnUsedAndMinNodeItem();
                    if (item == null) break;

                    //遍历过的做标记
                    item.used = true;
                    //获取当前点的距离行表
                    var tempRow = GetRowArray(item.index);
                    foreach (var noteItem in m_NodeItems)
                    {
                        //计算新路径如果比之前记录的短，则更新之前记录的数据
                        if (noteItem.weight > tempRow[noteItem.index] + item.weight)
                        {
                            noteItem.weight = tempRow[noteItem.index] + item.weight;
                            noteItem.roadNodes.Clear();
                            noteItem.roadNodes.AddRange(item.roadNodes);
                            noteItem.roadNodes.Add(item.node);
                        }
                    }
                }

                //从所有路径中找出到达目标点的路径
                var desNoteItem = m_NodeItems.FirstOrDefault(n => n.node.id == dNode.id);
                //如果路径参加过计算，且是可以到达的
                if (desNoteItem.used && desNoteItem.weight < float.MaxValue)
                {
                    var edges = new List<Edge>();
                    for (int i = 0; i < desNoteItem.roadNodes.Count - 1; i++)
                    {
                        var desNode = desNoteItem.roadNodes[i];
                        var desNextNode = desNoteItem.roadNodes[i + 1];
                        edges.Add(GetFirstOrDefaultEdge(desNode.id, desNextNode.id));
                    }
                    edges.Add(GetFirstOrDefaultEdge(desNoteItem.roadNodes.Last().id, dNode.id));
                    return edges;
                }
                return null;
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                m_NodeItems.ForEach(n => { n.used = false; n.roadNodes.Clear(); });
            }
        }

        /// <summary>
        /// 获取列表对应行各点最近距离
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private float[] GetRowArray(int row)
        {
            float[] result = new float[m_Graph.GetLength(1)];
            foreach (var index in Enumerable.Range(0, result.Length))
            {
                result[index] = m_Graph[row, index];
            }
            return result;
        }

        private NodeItem GetUnUsedAndMinNodeItem()
        {
            return m_NodeItems.Where(x => !x.used && x.weight != float.MaxValue).OrderBy(x => x.weight).FirstOrDefault();
        }

        private Edge GetFirstOrDefaultEdge(int startId, int endId)
        {
            return m_Edges.FirstOrDefault(e => e.IsMatch(startId, endId));
        }
    }
}