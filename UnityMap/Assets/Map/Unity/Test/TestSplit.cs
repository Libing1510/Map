using UnityEngine;
using YJ.Map;

namespace YJ.Unity
{
    public class TestSplit : MonoBehaviour
    {
        public Vector3 vert;

        private void Update()
        {
            Plane plane = new Plane(Vector3.up, Vector3.up * -0.1f);
            var normal = plane.normal.normalized;

            YJPlane yjPlane = new YJPlane(System.Numerics.Vector3.UnitY, System.Numerics.Vector3.UnitY * -0.1f);
            var yjNormal = yjPlane.normal.ToNormalized();

            Vector3 point = plane.normal * -plane.distance;
            var above = Vector3.Dot(vert - point, normal) >= 0f;
            var yjPoint = yjPlane.normal * -yjPlane.distance;
            var yjVert = new System.Numerics.Vector3(vert.x, vert.y, vert.z);
            var yjAbove = System.Numerics.Vector3.Dot(yjVert - yjPoint, yjNormal) >= 0f;
            Debug.Log($"plane:{plane.normal}/{normal},dis ={plane.distance},above={above}," +
                $"\nyjPlane:{yjPlane.normal}/{yjNormal},dis={yjPlane.distance},abv={yjAbove}");
        }
    }
}