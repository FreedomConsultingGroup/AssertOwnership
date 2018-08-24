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
            /* Called by AssertOwnershipController, points to the handlers for the static
               based on the path of the request*/
            
            string filePath = Global.StaticDirectory;

            for (int i = index; i < path.Length; i++)
            {
                filePath += @"\" + path[i];
            }

            if (IsInvalid(filePath))
            {
                // Throw 404 Exception
                context.Response.StatusCode = 404;
                return;
            }

            try
            {
                // try to find the file, return 404 if not found
                context.Response.Write(File.ReadAllText(filePath));
                return;
            }
            catch (IOException e)
            {
                // Throw 404 exception
                context.Response.StatusCode = 404;
                return;
            }
        }

        private bool IsInvalid(string filePath)
        {
            /* Returns false (meaning the file path is valid) only if:
                    The requested extension is allowed
                    The file exists
                    The file is within the static directory */

            string[] acceptedExtensions = { ".html", ".css", ".js" };
            foreach (string extension in acceptedExtensions)
            {
                if (filePath.EndsWith(extension))
                {
                    try
                    {
                        FileInfo file = new FileInfo(filePath);
                        if (file.FullName.ToLower().StartsWith(Global.StaticDirectory.ToLower()))
                        {
                            return false;
                        }
                    }catch(IOException e)
                    {
                        return true;
                    }
                }
            }
            return true;
        }
    }
}