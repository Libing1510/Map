using System;
using System.Collections.Generic;
using System.Threading;

namespace YJ.Map.SplitMesh
{
    public class SplitMeshJob
    {
        public MeshInfo meshInfo { get; private set; }
        public YJPlane plane { get; private set; }
        public ManualResetEvent doneEvent { get; private set; }
        public YJRect uvRange { get; private set; }
        public int id { get; private set; }
        public List<MeshInfo> aboveResult { get; private set; }
        public List<MeshInfo> belowResult { get; private set; }
        public string exception { get; private set; }

        public bool aboveSplitConnection { get; private set; }
        public bool belowSplitConnection { get; private set; }
        private Action<int, List<MeshInfo>, List<MeshInfo>, MeshInfo> m_OnComplete;

        public SplitMeshJob(int id, YJPlane plane, MeshInfo meshInfo, YJRect uvRange, bool aboveSplitConnection, bool belowSplitConnection, Action<int, List<MeshInfo>, List<MeshInfo>, MeshInfo> onComplete)
        {
            this.plane = plane;
            this.meshInfo = meshInfo;
            this.uvRange = uvRange;
            this.id = id;
            doneEvent = new ManualResetEvent(false);
            this.m_OnComplete = onComplete;
            this.aboveSplitConnection = aboveSplitConnection;
            this.belowSplitConnection = belowSplitConnection;
        }

        public void ThreadPoolCallback(System.Object obj)
        {
            MeshInfo cut = null;
            try
            {
                meshInfo.VertexRemovalRepeat();

                DateTime t = DateTime.Now;
                SplitMesh splitMesh = new SplitMesh(meshInfo, uvRange);
                var rt = splitMesh.Split(plane, out cut);
                DateTime t2 = DateTime.Now;
                Logger.Info($"split {meshInfo.name} mesh {(t2 - t).TotalSeconds}");
                aboveResult = new List<MeshInfo>();
                belowResult = new List<MeshInfo>();
                var a = rt[0];
                a.name = $"{meshInfo.name}_above";
                var b = rt[1];
                b.name = $"{meshInfo.name}_below";
                Logger.Debug($"ThreadPoolCallback: a:{a.vertices.Count},{a.triangles.Count} b:{b.vertices.Count},{b.triangles.Count}");
                if (a.vertices.Count > 3)
                {
                    if (aboveSplitConnection)
                    {
                        SplitMeshByConnection above = new SplitMeshByConnection(a);
                        var result = above.SplitConnection();
                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i].name = $"{a.name}_{i}";
                            if (result[i].vertices.Count > 3)
                            {
                                aboveResult.Add(result[i]);
                            }
                        }
                    }
                    else
                    {
                        aboveResult.Add(a);
                    }
                }

                if (b.vertices.Count > 3)
                {
                    if (belowSplitConnection)
                    {
                        SplitMeshByConnection below = new SplitMeshByConnection(b);
                        var result = below.SplitConnection();
                        for (int i = 0; i < result.Count; i++)
                        {
                            result[i].name = $"{b.name}_{i}";
                            if (result[i].vertices.Count > 3)
                            {
                                belowResult.Add(result[i]);
                            }
                        }
                    }
                    else
                    {
                        belowResult.Add(b);
                    }
                }
                if (aboveSplitConnection || belowSplitConnection)
                    Logger.Debug($"split {meshInfo.name} connection mesh {(DateTime.Now - t2).TotalSeconds},totalTime={(DateTime.Now - t).TotalSeconds},above:{aboveResult.Count},below:{belowResult.Count}");
            }
            catch (Exception ex)
            {
                exception = ex.ToString();
            }
            m_OnComplete?.Invoke(id, aboveResult, belowResult, cut);
            doneEvent.Set();
        }
    }
}