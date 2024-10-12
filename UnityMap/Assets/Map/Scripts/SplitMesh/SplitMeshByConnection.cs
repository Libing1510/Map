using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace YJ.Map.SplitMesh
{
    public class SplitMeshByConnection
    {
        private List<int> m_AllTriangle = new List<int>();
        private List<int> m_NewTriangle = new List<int>();
        private List<int> restTriangle = new List<int>();
        private List<Vector3> newNormal = new List<Vector3>();
        private List<Vector2> newUV = new List<Vector2>();
        private List<Vector4> newTangent = new List<Vector4>();

        private MeshInfo meshInfo;

        public SplitMeshByConnection(MeshInfo meshInfo)
        {
            this.meshInfo = meshInfo;
        }

        public List<MeshInfo> SplitConnection(int maxChildrenMesh = 50, int maxForEachCount = 50)
        {
            List<int> allTriangleList = new List<int>();
            List<int> resetTriangleList = new List<int>();
            List<int> newTriangleList = new List<int>();
            allTriangleList.AddRange(meshInfo.triangles);
            resetTriangleList.AddRange(allTriangleList);

            List<MeshInfo> resultMeshs = new List<MeshInfo>();

            int whileCount = maxChildrenMesh;
            while (whileCount > 0 && resetTriangleList.Count > 0)
            {
                newTriangleList.Clear();
                //得到第一个三角形
                for (int i = 0; i < 3; i++)
                {
                    newTriangleList.Add(resetTriangleList[i]);
                }
                int maxForeachCount = maxForEachCount;
                List<int> ruledOutId = new List<int>();
                bool removeResult = false;
                do
                {
                    //遍历剩下的三角形
                    var re = ForEachTriangles(ruledOutId);

                    //将刚刚拆分出去的三角形从下一次循环中移除
                    removeResult = RemoveSplit();
                    maxForeachCount--;
                    ruledOutId.Clear();
                    ruledOutId = re;
                } while (!removeResult && maxForeachCount > 0);

                //生成裁切的网格信息
                var newMesh = NewSplitMeshInfo();
                if (newMesh.vertices.Count < 4)
                {
                    //Debug.Log("此区块只有4个点，无法形成模型,故舍弃");
                    continue;
                }
                resultMeshs.Add(newMesh);
                //Debug.Log($"SplitConnection:whileCount:{whileCount}/{count},{resetTriangleList.Count},meshCout:{resultMeshs.Count}");
                whileCount--;
            }

            return resultMeshs;

            //遍历剩下的三角形
            List<int> ForEachTriangles(List<int> ruledOut)
            {
                List<int> triangleIndex = new List<int>();
                if (ruledOut.Count > 0)
                {
                    ruledOut.ForEach(IsConnect);
                }
                else
                {
                    for (int i = 1; i < resetTriangleList.Count / 3; i++)
                    {
                        IsConnect(i);
                    }
                }
                return triangleIndex;

                void IsConnect(int i)
                {
                    //判断该三角形与newTriangleList是否有共用的顶点，若有，就该三角形的所有顶点加入newTriangleList
                    var aIndex = i * 3;
                    var bIndex = i * 3 + 1;
                    var cIndex = i * 3 + 2;
                    if (newTriangleList.Contains(resetTriangleList[aIndex])
                       || newTriangleList.Contains(resetTriangleList[bIndex])
                       || newTriangleList.Contains(resetTriangleList[cIndex]))
                    {
                        newTriangleList.Add(resetTriangleList[aIndex]);
                        newTriangleList.Add(resetTriangleList[bIndex]);
                        newTriangleList.Add(resetTriangleList[cIndex]);
                    }
                    else
                    {
                        triangleIndex.Add(i);
                    }
                }
            }

            //将刚刚拆分出去的三角形从下一次循环中移除
            bool RemoveSplit()
            {
                resetTriangleList.Clear();
                for (int n = 0; n < allTriangleList.Count; n++)
                {
                    if (!newTriangleList.Contains(allTriangleList[n]))
                    {
                        resetTriangleList.Add(allTriangleList[n]);
                    }
                    else
                    {
                        var temp = allTriangleList[n];
                    }
                }
                //Debug.Log($"temp={allTriangleList.Count}-{newTriangleList.Count}={allTriangleList.Count - newTriangleList.Count}/{resetTriangleList.Count}={allTriangleList.Count - newTriangleList.Count - resetTriangleList.Count}");

                bool result = allTriangleList.Count - resetTriangleList.Count == newTriangleList.Count;
                if (result)
                {
                    allTriangleList.Clear();
                    allTriangleList.AddRange(resetTriangleList);
                }
                else
                {
                    resetTriangleList.Clear();
                    resetTriangleList.AddRange(allTriangleList);
                }
                return result;
            }

            //生成裁切的网格信息
            MeshInfo NewSplitMeshInfo()
            {
                //1.创建新数组verIndMap，将newTriangleList去重复，并从小到大排序的结果填入
                // 注：此时，处理完verIndMap为splitedVerts与verts之间的映射表，顶点splitedVerts
                List<int> verIndMap = new List<int>();
                foreach (int index in newTriangleList)//去重复
                {
                    if (!verIndMap.Contains(index))
                        verIndMap.Add(index);
                }
                verIndMap.Sort();//排序

                //2.我们要创建splitedVerts(分离出来的顶点的数组)
                Vector3[] splitedVerts = new Vector3[verIndMap.Count];
                Vector2[] splitedUvs = new Vector2[verIndMap.Count];
                Vector3[] splitedNormals = new Vector3[verIndMap.Count];
                Vector4[] splitedTangent = new Vector4[verIndMap.Count];

                //有了verIndMap的帮助，我们可以遍历verIndMap将Verts中我们所需要的顶点信息加入splitedVerts中
                for (int i = 0; i < verIndMap.Count; i++)
                {
                    var index = verIndMap[i];
                    splitedVerts[i] = meshInfo.vertices[index];
                    if (meshInfo.uvs.Count > index)
                        splitedUvs[i] = meshInfo.uvs[index];
                    if (meshInfo.normals.Count > index)
                        splitedNormals[i] = meshInfo.normals[index];
                    if (meshInfo.tangents.Count > index)
                        splitedTangent[i] = meshInfo.tangents[index];
                }

                //3.我们要创建splitedIndices(分离出来的Indice的数组)
                int[] splitedIndices = new int[newTriangleList.Count];
                //有了verIndMap的帮助，根据newnewIndices得到对应splitedVerts的新的索引信息splitedIndices
                for (int i = 0; i < newTriangleList.Count; i++)
                {
                    for (int j = 0; j < verIndMap.Count; j++)
                    {
                        if (newTriangleList[i] == verIndMap[j])
                            splitedIndices[i] = j;
                    }
                }

                var mi = new MeshInfo(splitedVerts, splitedIndices, splitedNormals, splitedUvs, splitedTangent, meshInfo.size, meshInfo.center);
                return mi;
            }
        }
    }
}