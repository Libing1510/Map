using System;
using System.Numerics;

namespace YJ.Map
{
    public static class VectorExtension
    {
        public static Vector3 ToSnVector3(this Vector2 val)
        {
            return new Vector3(val.X, 0, val.Y);
        }

        public static Vector2 ToSnVector2(this Vector3 val)
        {
            return new Vector2(val.X, val.Z);
        }

        public static Vector3 ToNormalized(this Vector3 val)
        {
            return val / val.Length();
        }

        public static Vector2 ToNormalized(this Vector2 val)
        {
            return val / val.Length();
        }

        public static Vector4 ToNormalized(this Vector4 val)
        {
            return val / val.Length();
        }

        public static VoronatorSharp.Vector2 ToVSVector2(this Clipper2Lib.PointD val)
        {
            return new VoronatorSharp.Vector2((float)val.x, (float)val.y);
        }

        public static Vector2 ToSnVector2(this Clipper2Lib.PointD val)
        {
            return new Vector2((float)val.x, (float)val.y);
        }

        public static Vector2 ToSnVector2(this VoronatorSharp.Vector2 val)
        {
            return new Vector2(val.x, val.y);
        }

        public static Vector4 ToSnVector4(this Assimp.Matrix3x3 matrix3X3)
        {
            var values = new float[,]
            {
                {matrix3X3.A1,matrix3X3.A2,matrix3X3.A3},
                {matrix3X3.B1,matrix3X3.B2,matrix3X3.B3},
                {matrix3X3.C1,matrix3X3.C2,matrix3X3.C3}
                
            };

            var trace = values[0, 0] + values[1, 1] + values[2, 2];
            float w, x, y, z;

            if (trace > 0)
            {
                var s = (float)Math.Sqrt(trace + 1.0f) * 2; // S=4*qw 
                w = 0.25f * s;
                x = (values[2, 1] - values[1, 2]) / s;
                y = (values[0, 2] - values[2, 0]) / s;
                z = (values[1, 0] - values[0, 1]) / s;
            }
            else if ((values[0, 0] > values[1, 1]) && (values[0, 0] > values[2, 2]))
            {
                var s = (float)Math.Sqrt(1.0f + values[0, 0] - values[1, 1] - values[2, 2]) * 2; // S=4*qx
                w = (values[2, 1] - values[1, 2]) / s;
                x = 0.25f * s;
                y = (values[0, 1] + values[1, 0]) / s;
                z = (values[0, 2] + values[2, 0]) / s;
            }
            else if (values[1, 1] > values[2, 2])
            {
                var s = (float)Math.Sqrt(1.0f + values[1, 1] - values[0, 0] - values[2, 2]) * 2; // S=4*qy
                w = (values[0, 2] - values[2, 0]) / s;
                x = (values[0, 1] + values[1, 0]) / s;
                y = 0.25f * s;
                z = (values[1, 2] + values[2, 1]) / s;
            }
            else
            {
                var s = (float)Math.Sqrt(1.0f + values[2, 2] - values[0, 0] - values[1, 1]) * 2; // S=4*qz
                w = (values[1, 0] - values[0, 1]) / s;
                x = (values[0, 2] + values[2, 0]) / s;
                y = (values[1, 2] + values[2, 1]) / s;
                z = 0.25f * s;
            }

            return new Vector4(w, x, y, z);
            
            
            
        }

        public static float GetAngle(this Vector2 a, Vector2 b)
        {
            // 计算差向量
            Vector2 difference = b - a;

            // 使用 Math.Atan2 计算角度
            float angleInRadians = MathF.Atan2(difference.Y, difference.X);

            // 转换为度数
            float angleInDegrees = angleInRadians * (180f / MathF.PI);
        
            return angleInDegrees;
        }
    }
}