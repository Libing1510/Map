using SNVector2 = System.Numerics.Vector2;
using SNVector3 = System.Numerics.Vector3;
using SNVector4 = System.Numerics.Vector4;
using UnityEngine;

namespace YJ.Unity.Extension
{
    public static class VectorExtension
    {
        public static Vector2 ToUVector2(this SNVector2 val) => new Vector2(val.X, val.Y);

        public static Vector3 ToUVector3(this SNVector3 val) => new Vector3(val.X, val.Y, val.Z);

        public static Vector4 ToUVector4(this SNVector4 val) => new Vector4(val.X, val.Y, val.Z, val.W);

        public static Vector2 ToUVector3(this SNVector2 val) => new Vector3(val.X, 0, val.Y);

        public static SNVector2 ToSnVector2(this Vector2 val) => new SNVector2(val.x, val.y);

        public static SNVector3 ToSnVector3(this Vector3 val) => new SNVector3(val.x, val.y, val.z);

        public static SNVector4 ToSnVector4(this Vector4 val) => new SNVector4(val.x, val.y, val.z, val.w);

        public static SNVector2 ToSnVector2(this Vector3 val) => new SNVector2(val.x, val.z);

        public static Quaternion ToUQuaternion(this SNVector4 val) => new Quaternion(val.X,val.Y,val.Z,val.W);
    }
}