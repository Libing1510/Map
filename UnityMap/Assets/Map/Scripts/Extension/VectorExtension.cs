namespace YJ.Map
{
    public static class VectorExtension
    {
        public static System.Numerics.Vector3 ToSNVector3(this System.Numerics.Vector2 val)
        {
            return new System.Numerics.Vector3(val.X, 0, val.Y);
        }

        public static System.Numerics.Vector2 ToSNVector2(this System.Numerics.Vector3 val)
        {
            return new System.Numerics.Vector2(val.X, val.Z);
        }

        public static System.Numerics.Vector3 ToNormalized(this System.Numerics.Vector3 val)
        {
            return val / val.Length();
        }

        public static System.Numerics.Vector2 ToNormalized(this System.Numerics.Vector2 val)
        {
            return val / val.Length();
        }

        public static System.Numerics.Vector4 ToNormalized(this System.Numerics.Vector4 val)
        {
            return val / val.Length();
        }

        public static VoronatorSharp.Vector2 ToVSVector2(this Clipper2Lib.PointD val)
        {
            return new VoronatorSharp.Vector2((float)val.x, (float)val.y);
        }

        public static System.Numerics.Vector2 ToSNVector2(this Clipper2Lib.PointD val)
        {
            return new System.Numerics.Vector2((float)val.x, (float)val.y);
        }

        public static System.Numerics.Vector2 ToSNVector2(this VoronatorSharp.Vector2 val)
        {
            return new System.Numerics.Vector2(val.x, val.y);
        }
    }
}