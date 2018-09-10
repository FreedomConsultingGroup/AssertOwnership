using System.Web;

namespace FCG.AssertOwnership
{
    public abstract class RestHttpHandler
    {
        private string path;

        public RestHttpHandler(string path)
        {
            this.path = path;
        }

        public string Path { get { return path; } }

        public abstract void ProcessRequest(HttpContext context);
    }
}