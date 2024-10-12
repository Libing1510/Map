using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace YJ.Map.SplitMesh
{
    public class SplitMeshThreadPool
    {
        private List<MeshInfo> m_MeshList;
        private YJRect uvRange;
        private YJPlane plane;

        public SplitMeshThreadPool(YJPlane plane, YJRect rect, IEnumerable<MeshInfo> meshInfos)
        {
            this.plane = plane;
            this.uvRange = rect;
            m_MeshList = meshInfos.ToList();
        }

        public void ThreadDoSplitMesh(bool aboveSplitConnection, bool belowSplitConnection, Action<int, List<MeshInfo>, List<MeshInfo>, MeshInfo> onComplete)
        {
            List<SplitMeshJob> jobList = new List<SplitMeshJob>();
            List<ManualResetEvent> eventList = new List<ManualResetEvent>();
            int timeout = 120000;
            for (int i = 0; i < m_MeshList.Count; i++)
            {
                var job = new SplitMeshJob(i, plane, m_MeshList[i], uvRange, aboveSplitConnection, belowSplitConnection, onComplete);

                jobList.Add(job);
                eventList.Add(job.doneEvent);
                ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);
                if (eventList.Count > Environment.ProcessorCount)
                {
                    WaitForDoSplitMesh(eventList, timeout);
                }
            }

            while (eventList.Count > 0)
            {
                WaitForDoSplitMesh(eventList, timeout);
            }

            jobList.ForEach(job =>
            {
                if (string.IsNullOrEmpty(job.exception)) return;
                Logger.Error($"{job.id}:{job.exception}");
            });
        }

        private void WaitForDoSplitMesh(List<ManualResetEvent> events, int timeout)
        {
            int finished = WaitHandle.WaitAny(events.ToArray(), timeout);
            if (finished == WaitHandle.WaitTimeout)
            {
                //超时
                Logger.Error("split mesh time out");
            }
            events.RemoveAt(finished);
        }
    }
}