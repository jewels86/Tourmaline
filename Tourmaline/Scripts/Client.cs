namespace Tourmaline.Scripts 
{
    public class Client 
    {
        private string url;
        private int rateLimit;

        public string URL { get { return url; } }
        public int RateLimit { get { return rateLimit; } }

        public Client(string url, int rateLimit = 60) 
        {
            this.url = url;
            this.rateLimit = rateLimit;
        }

        public List<string> Start() {}
    }
}