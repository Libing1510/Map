using System.ComponentModel;
using System.Drawing;
using System;
using UnityEngine;

namespace OuelletConvexHull
{
    [Serializable]
    [TypeConverter(typeof(PointConverter))]
    public struct Point : IFormattable
    {
        internal double _x;

        internal double _y;

        public double X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public static bool operator ==(Point point1, Point point2)
        {
            if (point1.X == point2.X)
            {
                return point1.Y == point2.Y;
            }

            return false;
        }

        public static bool operator !=(Point point1, Point point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(Point point1, Point point2)
        {
            if (point1.X.Equals(point2.X))
            {
                return point1.Y.Equals(point2.Y);
            }

            return false;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Point point))
            {
                return false;
            }

            return Equals(this, point);
        }

        public bool Equals(Point value)
        {
            return Equals(this, value);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public Point(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public Vector3 toVector3()
        {
            return new Vector3((float)_x, 0, (float)_y);
        }

        public static Vector2 operator -(Point point1, Point point2)
        {
            return new Vector2((float)(point1._x - point2._x), (float)(point1._y - point2._y));
        }

        public static Point operator +(Point point, Point vector)
        {
            return new Point(point._x + vector._x, point._y + vector._y);
        }

        public static Point operator /(Point point, float vector)
        {
            return new Point(point._x / vector, point._y / vector);
        }

        public void Offset(double offsetX, double offsetY)
        {
            _x += offsetX;
            _y += offsetY;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return $"({_x},{_y})";
        }
    }
}