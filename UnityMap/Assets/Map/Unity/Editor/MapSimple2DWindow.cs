using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YJ.Map;
using YJ.Map.Clip;
using YJ.Map.DelaunatorVoronator;
using YJ.Unity.Extension;

namespace YJ.Unity.Editor
{
    public class MapSimple2DWindow : EditorWindow
    {
        #region Property

        private GameObject m_TargetGO;
        private string m_TargetPath;
        private float m_groundHigh = 0.1f;
        private float m_wallHigh = 1.0f;
        private float m_wallMinHigh = 1.0f;
        private float m_wallExtend = 0.1f;
        private bool m_error;
        private object m_Lock;
        private GameObject m_wallGO;
        private GameObject m_groundGO;
        private Material m_wallMat;
        private Material m_groundMat;
        private Material m_originMat;
        private Queue<NewMesh> m_CreateQueue;

        #endregion Property

        #region Create Window

        [MenuItem("Tools/Simple2DMap")]
        public static void SplitMapMesh()
        {
            var window = GetWindow(typeof(MapSimple2DWindow));
            window.Show();
        }

        public MapSimple2DWindow()
        {
            m_Lock = new object();
            m_CreateQueue = new Queue<NewMesh>();
            titleContent = new GUIContent();
            titleContent.text = "简单二维地图生成窗口";
        }

        #endregion Create Window

        private void OnGUI()
        {
            m_error = false;
            SelectTargetModel();
            SplitParameters();
            SplitButton();

            lock (m_Lock)
            {
                while (m_CreateQueue.Count > 0)
                {
                    CreateGO(m_CreateQueue.Dequeue());
                }
            }
        }

        #region UI

        private void SelectTargetModel()
        {
            GUILayout.Space(10);
            GUILayout.Label("扫描地图Model:", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal("box");
            if (GUILayout.Button("选择文件"))
                SelectFile();
            GUILayout.TextField(m_TargetPath);
            GUILayout.EndHorizontal();
            m_TargetGO = EditorGUILayout.ObjectField(m_TargetGO, typeof(GameObject), false) as GameObject;
            if (string.IsNullOrEmpty(m_TargetPath) && m_TargetGO == null)
                ShowError("请先选择地图Model ！！！");
        }

        private void SplitParameters()
        {
            GUILayout.Space(10);

            GUILayout.Label("配置拆分参数", EditorStyles.boldLabel);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("地面最大高度");
            m_groundHigh = EditorGUILayout.FloatField(m_groundHigh);
            GUILayout.EndHorizontal();
            if (m_groundHigh < 0.02f)
                ShowWarning("确认地面有这么低吗 ？？？");

            GUILayout.Space(1);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("墙面最小高度");
            m_wallMinHigh = EditorGUILayout.FloatField(m_wallMinHigh);
            m_wallHigh = m_groundHigh + m_wallMinHigh;
            GUILayout.EndHorizontal();
            if (m_wallMinHigh < 0.1f)
                ShowError("墙面最小高度过小了 ！！！");

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("墙面外扩距离");
            m_wallExtend = EditorGUILayout.FloatField(m_wallExtend);
            m_wallExtend = Mathf.Clamp(m_wallExtend, 0.0f, 10);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("地面材质球");
            m_groundMat = EditorGUILayout.ObjectField(m_groundMat, typeof(Material), false) as Material;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("墙面材质球");
            m_wallMat = EditorGUILayout.ObjectField(m_wallMat, typeof(Material), false) as Material;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("原始Mesh材质球");
            m_originMat = EditorGUILayout.ObjectField(m_originMat, typeof(Material), false) as Material;
            GUILayout.EndHorizontal();
        }

        private void SelectFile()
        {
            // 打开文件选择对话框
            string path = EditorUtility.OpenFilePanel("Select File", "", "glb");
            if (!string.IsNullOrEmpty(path))
            {
                m_TargetPath = path; // 更新选择的路径
            }
        }

        private void SplitButton()
        {
            if (m_error) return;

            if (GUILayout.Button("开始生成"))
            {
                m_wallGO = new GameObject($"Walls");
                m_groundGO = new GameObject($"Grounds");
                CreateMap();
            }
        }

        private void ShowError(string msg)
        {
            EditorGUILayout.HelpBox(msg, MessageType.Error);
            m_error = true;
        }

        private void ShowWarning(string msg)
        {
            EditorGUILayout.HelpBox(msg, MessageType.Warning);
        }

        private void ShowInfo(string msg)
        {
            EditorGUILayout.HelpBox(msg, MessageType.Info);
        }

        #endregion UI

        private void CreateMap()
        {
            IEnumerable<MeshInfo> meshInfos;
            if (!string.IsNullOrEmpty(m_TargetPath))
            {
                if (!File.Exists(m_TargetPath))
                {
                    EditorUtility.DisplayDialog("错误", "选择文件不存在", "确认");
                    m_TargetPath = null;
                    return;
                }

                meshInfos = LoadMesh.LoadGLBMesh(m_TargetPath);
                Debug.Log($"加载到 {meshInfos.Count()}");

                var root = new GameObject("Root");
                foreach (var item in meshInfos)
                {
                    CreateGO(item, root, false);
                }
            }
            else
            {
                var meshFilter = m_TargetGO.GetComponentsInChildren<MeshFilter>();
                if (meshFilter.Length == 0)
                {
                    EditorUtility.DisplayDialog("错误", "地图模型下没有MeshFilter", "重新选择");
                    m_TargetGO = null;
                    return;
                }
                else
                {
                    meshInfos = meshFilter.Select(mf => ShowMeshTools.GetMeshInfo(mf.sharedMesh));
                }
            }

            var mesh2map = new Mesh2Map(m_groundHigh, m_wallHigh, m_wallExtend);
            mesh2map.ThreadSplit(meshInfos, OnWallComplete, OnGroundComplete, OnGround, OnWall);
            //mesh2map.ThreadSplit(new MeshInfo[] { meshInfos.ElementAt(2) }, OnWallComplete, OnGroundComplete, OnGround, OnWall);

            SavePolygon(mesh2map.GroundPolygon, $"{Application.dataPath}/ground.txt");
            SavePolygon(mesh2map.WallPolygon, $"{Application.dataPath}/wall.txt");

            if (mesh2map.RoadEdge.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Create map failed", "ok");
            }
            else
            {
                var lineStrs = mesh2map.RoadEdge.Select(line => line.ToString()).ToArray();
                File.WriteAllLines($"{Application.dataPath}/road.txt", lineStrs);
            }

            void OnWallComplete(int id, List<System.Numerics.Vector3> vertices, string name)
            {
                var verts = vertices.Select(v => v.ToUVector3());
                var vects = vertices.Select(v => v.ToSNVector2());
                lock (m_Lock)
                {
                    m_CreateQueue.Enqueue(new NewMesh(id, false, verts, name, vects));
                }
            }
            void OnGroundComplete(int id, List<System.Numerics.Vector3> vertices, string name)
            {
                var verts = vertices.Select(v => v.ToUVector3());
                var vects = vertices.Select(v => v.ToSNVector2());
                lock (m_Lock)
                {
                    m_CreateQueue.Enqueue(new NewMesh(id, true, verts, name, vects));
                }
            }

            void OnWall(List<MeshInfo> meshInfos)
            {
                var splitWallRoot = new GameObject("SplitWallRoot");
                foreach (var item in meshInfos)
                {
                    CreateGO(item, splitWallRoot, false);
                }
            }

            void OnGround(List<MeshInfo> meshInfos)
            {
                var splitGroundRoot = new GameObject("SplitGroundRoot");
                foreach (var item in meshInfos)
                {
                    CreateGO(item, splitGroundRoot, true);
                }
            }
        }

        private void SavePolygon(Polygon polygon, string path)
        {
            if (polygon == null) return;
            var vectors = polygon.GetVectors();
            var lines = new List<string>();
            vectors.ForEach(vects =>
            {
                var strs = vects.Select(v => $"{v.X},{v.Y}");
                lines.Add(string.Join("!", strs));
            });
            File.WriteAllLines(path, lines.ToArray());
        }

        private GameObject CreateGO(MeshInfo meshInfo, GameObject parent, bool isGround)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = string.IsNullOrEmpty(meshInfo.name) ? "origin" : meshInfo.name;
            ShowMeshTools.UpdateMesh(go, new MeshInfo[] { meshInfo });
            go.transform.SetParent(parent.transform);
            if (m_originMat != null)
                go.GetComponent<MeshRenderer>().material = m_originMat;
            return go;
        }

        private GameObject CreateGO(NewMesh newMesh)
        {
            if (newMesh.vertices.Count() < 3)
            {
                Debug.LogWarning($"{newMesh.name} has {newMesh.vertices.Count()}");
                return null;
            }
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = $"{(newMesh.isGround ? "Gorund" : "Wall")}_{newMesh.id}__{newMesh.name}";

            //Mesh m = CreateMesh(newMesh.vertices.ToArray());
            Mesh m = CreateMesh2(newMesh.vect2s.ToArray());
            var mf = go.GetComponent<MeshFilter>();
            mf.sharedMesh = m;
            if (newMesh.isGround && m_groundMat != null)
            {
                go.GetComponent<MeshRenderer>().material = m_groundMat;
            }
            else if (!newMesh.isGround && m_wallMat != null)
            {
                go.GetComponent<MeshRenderer>().material = m_wallMat;
            }

            go.transform.SetParent(newMesh.isGround ? m_groundGO.transform : m_wallGO.transform);
            go.transform.localPosition = Vector3.up * m_groundHigh;
            return go;

            Mesh CreateMesh2(System.Numerics.Vector2[] points)
            {
                Polygon polygon = new Polygon(points);
                var mi = DelaunatorVoronatorTools.PoygonToMesh(polygon).ToArray();
                Mesh mesh = new Mesh();
                mesh.vertices = mi[0].vertices.ToList().Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
                mesh.triangles = mi[0].triangles.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                return mesh;
            }

            Mesh CreateMesh(Vector3[] points)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = points;
                int[] triangles = new int[(points.Length - 2) * 3];
                Vector3[] normals = new Vector3[points.Length];
                for (int i = 0; i < points.Length - 2; i++)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                    normals[i] = Vector3.up;
                }
                normals[^1] = Vector3.up;
                normals[^2] = Vector3.up;
                mesh.triangles = triangles;
                mesh.normals = normals;
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}