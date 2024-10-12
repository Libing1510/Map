using System.Numerics;

namespace YJ.Map
{
    public struct Line2D
    {
        public Vector2 aPoint;
        public Vector2 bPoint;

        public Line2D(Vector2 aPoint, Vector2 bPoint)
        {
            this.aPoint = aPoint;
            this.bPoint = bPoint;
        }

        public override readonly string ToString()
        {
            return $"{aPoint.X:f2},{aPoint.Y:f2},{bPoint.X:f2},{bPoint.Y:f2}";
        }
    }
}