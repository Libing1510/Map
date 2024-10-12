using System.Numerics;

namespace YJ.Map
{
    public struct YJPlane
    {
        internal const int size = 16;

        private Vector3 m_Normal;

        private float m_Distance;

        public Vector3 normal
        {
            get
            {
                return m_Normal;
            }
            set
            {
                m_Normal = value;
            }
        }

        public float distance
        {
            get
            {
                return m_Distance;
            }
            set
            {
                m_Distance = value;
            }
        }

        public YJPlane flipped
        {
            get
            {
                return new YJPlane(-m_Normal, 0f - m_Distance);
            }
        }

        public YJPlane(Vector3 inNormal, Vector3 inPoint)
        {
            m_Normal = Vector3.Normalize(inNormal);
            m_Distance = 0f - Vector3.Dot(m_Normal, inPoint);
        }

        public YJPlane(Vector3 inNormal, float d)
        {
            m_Normal = Vector3.Normalize(inNormal);
            m_Distance = d;
        }

        public YJPlane(Vector3 a, Vector3 b, Vector3 c)
        {
            m_Normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            m_Distance = 0f - Vector3.Dot(m_Normal, a);
        }

        public void SetNormalAndPosition(Vector3 inNormal, Vector3 inPoint)
        {
            m_Normal = Vector3.Normalize(inNormal);
            m_Distance = 0f - Vector3.Dot(m_Normal, inPoint);
        }

        public void Set3Points(Vector3 a, Vector3 b, Vector3 c)
        {
            m_Normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            m_Distance = 0f - Vector3.Dot(m_Normal, a);
        }

        public void Flip()
        {
            m_Normal = -m_Normal;
            m_Distance = 0f - m_Distance;
        }

        public void Translate(Vector3 translation)
        {
            m_Distance += Vector3.Dot(m_Normal, translation);
        }
    }
}