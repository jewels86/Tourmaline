namespace Tourmaline.Scripts
{
    public struct Path
    {
        public string URL;
        public int Status;
        public string Type;

        public override string ToString()
        {
            return $"{URL} ({Type} - {Status})";
        }
    }
}
