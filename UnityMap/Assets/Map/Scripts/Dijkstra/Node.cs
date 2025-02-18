using System.Numerics;

namespace YJ.Map.Dijkstra
{
    public class Node
    {
        public int id { get; private set; }
        public bool enable { get; set; }
        public Vector3 vector { get; private set; }

        public override string ToString()
        {
            return $"{id},{enable},{vector.X:f2},{vector.Y:f2},{vector.Z:f2}";
        }

        public void UpdateVector(Vector3 v) => vector = v;

        public Node()
        {
            id = -1;
            enable = false;
            vector = Vector3.Zero;
        }

        public Node(int id, Vector3 vector)
        {
            this.id = id;
            this.vector = vector;
            this.enable = true;
        }
    }
}