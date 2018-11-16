using System.Web;

namespace FCG.AssertOwnership
{
    public abstract class APIHttpHandler
    {
        private string path;

        public APIHttpHandler(string path)
        {
            this.path = path;
        }

        public string Path { get { return path; } }

        public abstract void ProcessRequest(HttpContext context);
    }
}