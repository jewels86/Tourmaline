namespace Tourmaline.Scripts
{
    public struct Path
    {
        public string URL;
        public int Status;
        public string Type;

        public uint? Stray;

        public override string ToString()
        {
            return $"{URL} ({Type} - {Status})";
        }
    }
    public struct QueuedPath
    {
        public string URL;
        public Path? Parent;

        public QueuedPath(string url)
        {
            URL = url;
        }
    }
}
