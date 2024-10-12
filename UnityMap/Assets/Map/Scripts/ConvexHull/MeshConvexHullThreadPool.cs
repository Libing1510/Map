using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace YJ.Map.ConvexHull
{
    public class MeshConvexHullThreadPool
    {
        public void ThreadDoConvexHullMesh(List<MeshInfo> meshInfos, float extend, Action<int, List<Vector3>, string> onComplete)
        {
            List<MeshConvexHullJob> jobList = new List<MeshConvexHullJob>();
            List<ManualResetEvent> eventList = new List<ManualResetEvent>();
            int timeout = 900000;
            Logger.Debug($"MeshConvexHullThreadPool:{meshInfos.Count}");
            for (int i = 0; i < meshInfos.Count; i++)
            {
                var job = new MeshConvexHullJob(i, meshInfos[i].vertices, extend, onComplete, meshInfos[i].name);
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
                Logger.Error("convex hull mesh time out");
            }
            events.RemoveAt(finished);
        }
    }
}