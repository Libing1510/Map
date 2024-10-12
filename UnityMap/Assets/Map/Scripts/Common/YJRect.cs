namespace YJ.Map
{
    public struct YJRect
    {
        private float m_XMin;

        private float m_YMin;

        private float m_Width;

        private float m_Height;
        public float xMin => m_XMin;
        public float yMin => m_YMin;

        public float xMax => m_XMin + m_Width;
        public float yMax => m_YMin + m_Height;

        public YJRect(float x, float y, float w, float h)
        {
            m_XMin = x;
            m_YMin = y;
            m_Width = w;
            m_Height = h;
        }
    }
}