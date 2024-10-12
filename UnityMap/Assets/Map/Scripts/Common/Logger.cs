namespace YJ.Map
{
    internal class Logger
    {
        public static void Info(string msg)
        {
#if UNITY_64
            UnityEngine.Debug.Log(msg);
#endif
        }

        public static void Debug(string msg)
        {
#if UNITY_64
            UnityEngine.Debug.Log(msg);
#endif
        }

        public static void Error(string msg)
        {
#if UNITY_64
            UnityEngine.Debug.LogError(msg);
#endif
        }

        public static void Warn(string msg)
        {
#if UNITY_64
            UnityEngine.Debug.LogWarning(msg);
#endif
        }
    }
}