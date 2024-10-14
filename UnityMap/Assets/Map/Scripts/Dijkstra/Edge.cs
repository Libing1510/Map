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

        public override string ToString()
        {
            return $"a={aNode},b={bNode},w={weigth}";
        }

        /// <summary>
        /// 看两个点是否是相连边
        /// </summary>
        /// <param name="aId">a点</param>
        /// <param name="bId">b点</param>
        /// <returns></returns>
        public bool IsMatch(int aId, int bId) => (aNode.id == aId && bNode.id == bId) || (aNode.id == bId && bNode.id == aId);

        public bool HasNode(Node node) => aNode.id == node.id || bNode.id == node.id;

        public float Edge2dAngle(Edge edge)
        {
            return (bNode.vector - aNode.vector).ToSnVector2()
                .GetAngle((edge.bNode.vector - edge.aNode.vector).ToSnVector2());
        }
    }
}