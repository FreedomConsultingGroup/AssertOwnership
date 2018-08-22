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
            string filePath = Config.StaticDirectory;

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

            if (!IsValid(filePath))
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

        private bool IsValid(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (!file.FullName.ToLower().StartsWith(Config.StaticDirectory.ToLower()))
            {
                return false;
            }
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