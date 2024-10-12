using OuelletConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace YJ.Map.ConvexHull
{
    internal class MeshConvexHullJob
    {
        private MeshConvexHull m_MeshConvexHull;
        private Action<int, List<Vector3>, string> m_onComplete;
        public ManualResetEvent doneEvent { get; private set; }
        public string exception { get; private set; }
        public int id { get; private set; }
        public float extend { get; private set; }
        public string name { get; private set; }

        public MeshConvexHullJob(int id, List<Vector3> vertices, float extend, Action<int, List<Vector3>, string> onComplete, string name = null)
        {
            this.id = id;
            m_MeshConvexHull = new MeshConvexHull(vertices);
            this.extend = extend;
            doneEvent = new ManualResetEvent(false);
            m_onComplete = onComplete;
            this.name = name;
        }

        public void ThreadPoolCallback(System.Object obj)
        {
            List<Vector3> hullVertices = null;
            Logger.Debug($"MeshConvexHullJob ");
            try
            {
                hullVertices = m_MeshConvexHull.ConvexHullMesh2D(extend);
            }
            catch (Exception ex)
            {
                exception = ex.ToString();
            }
            m_onComplete?.Invoke(id, hullVertices, name);
            doneEvent.Set();
        }
    }
}