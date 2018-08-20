using System.Web;

namespace FCG.AssertOwnership
{
    public class StaticContentController : AOController
    {
        public static string Path { get { return "static"; } }

        private OwnershipHelper helper = new OwnershipHelper();

        public void Defer(HttpContext context, string[] path, int index)
        {
            string filePath = @"C:\inetpub\wwwroot\portal\ownership\static\";

            if (path[index] == "js")
            {
                filePath += "js";
            }
            else if (path[index] == "css")
            {
                filePath += "css";
            }
            

        }
    }
}