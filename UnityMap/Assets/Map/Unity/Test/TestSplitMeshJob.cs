using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using YJ.Map;
using YJ.Map.SplitMesh;
using MeshInfo = YJ.Map.MeshInfo;

namespace YJ.Unity
{
    public class TestSplitMeshJob : MonoBehaviour
    {
        public string fileName;

        private void Start()
        {
            string json = File.ReadAllText($"{Application.dataPath}/{fileName}");
            var meshInfo = new MeshInfo(json);
            Debug.Log($"meshinfo v = {meshInfo.vertices.Count} t={meshInfo.triangles.Count} name={meshInfo.name}");

            meshInfo.VertexRemovalRepeat();

            SplitMesh splitMesh = new SplitMesh(meshInfo, new YJRect(0, 0, 1, 1));
            var mi = splitMesh.Split(new YJPlane(System.Numerics.Vector3.UnitY, 0.4f), out MeshInfo cutInfo);
            Debug.Log($"mi={mi.Length}");
        }
    }
}