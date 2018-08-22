using System.Web;
using System.IO;

namespace FCG.AssertOwnership
{
    public class StaticContentController : AOController
    {
        public static string Path { get { return "static"; } }

        private OwnershipHelper helper = new OwnershipHelper();

        public void Defer(HttpContext context, string[] path, int index)
        {
            string filePath = @"C:\inetpub\wwwroot\portal\ownership\static";

            if (path[index] == "js")
            {
                filePath += @"\js";
            }
            else if (path[index] == "css")
            {
                filePath += @"\css";
            }

            foreach(string part in path)
            {
                filePath += @"\" + part;
            }

            if (!HasValidExtension(filePath))
            {
                // Throw 403 Exception
            }

            try
            {
                context.Response.Write(File.ReadAllText(filePath));
            }
            catch(FileNotFoundException e)
            {
                // Throw 404 exception
            }
        }

        private bool HasValidExtension(string filePath)
        {
            string[] acceptedExtensions = { ".html", ".css", ".js" };
            foreach(string extension in acceptedExtensions)
            {
                if (filePath.EndsWith(extension))
                {
                    return true;
                }
            }
            return false;
        }
    }
}