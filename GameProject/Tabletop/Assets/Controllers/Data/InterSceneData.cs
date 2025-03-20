namespace Controllers.Data
{
    internal static class InterSceneData
    {
        public static bool ShouldHost = false;
        public static string ConnectionAddress = "127.0.0.1";

        public static void Reset()
        {
            ShouldHost = false;
            ConnectionAddress = "127.0.0.1";
        }
    }
}
