namespace YJ.Map.Dijkstra
{
    public class Edge
    {
        public Node aNode { get; private set; }
        public Node bNode { get; private set; }
        public float weigth { get; private set; }

        public Edge(Node aNode, Node bNode, float weigth)
        {
            this.aNode = aNode;
            this.bNode = bNode;
            this.weigth = weigth;
        }

        /// <summary>
        /// 看两个点是否是相连边
        /// </summary>
        /// <param name="aId">a点</param>
        /// <param name="bId">b点</param>
        /// <returns></returns>
        public bool IsMatch(int aId, int bId) => (aNode.id == aId && bNode.id == bId) || (aNode.id == bId && bNode.id == aId);
    }
}