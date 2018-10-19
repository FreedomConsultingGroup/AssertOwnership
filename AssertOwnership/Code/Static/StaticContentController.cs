using System.Web;
using System.IO;
using System.Collections.Specialized;
using System;

namespace FCG.AssertOwnership
{
    public class StaticContentController : AOController
    {
        public static string Path { get { return "static"; } }
        private NameValueCollection acceptedExtensions;
        private OwnershipHelper helper;

        public StaticContentController()
        {
            helper = OwnershipHelper.getInstance();
            acceptedExtensions = new NameValueCollection();
            acceptedExtensions.Add(".html", "text/html");
            acceptedExtensions.Add(".css", "text/css");
            acceptedExtensions.Add(".js", "application/javascript");
        }

        public void Defer(HttpContext context, string[] path, int index)
        {
            /* Called by AssertOwnershipController, points to the handlers for the static
               based on the path of the request*/
            string[] subPath = new string[path.Length - index];
            Array.Copy(path, index, subPath, 0, subPath.Length);
            string filePath = System.IO.Path.Combine(Global.StaticDirectory, System.IO.Path.Combine(subPath));

            if (IsInvalid(filePath))
            {
                // Throw 404 Exception
                context.Response.StatusCode = 404;
                Global.LogInfo("Status: 404 returned. Invalid file path, Unacceptable file type or file not found");
                return;
            }

            try
            {
                // try to find the file, return 404 if not found
                context.Response.ContentType = GetContentType(filePath);
                context.Response.Write(File.ReadAllText(filePath));
                return;
            }
            catch (IOException e)
            {
                // Throw 404 exception
                context.Response.StatusCode = 404;
                Global.LogInfo("Status: 404 returned, Invalid file path, no file found");
                return;
            }
        }

        private bool IsInvalid(string filePath)
        {
            /* Returns false (meaning the file path is valid) only if:
                    The requested extension is allowed
                    The file exists
                    The file is within the static directory */

            foreach (string ext in acceptedExtensions.AllKeys)
            {
                if (filePath.EndsWith(ext))
                {
                    try
                    {
                        FileInfo file = new FileInfo(filePath);
                        if (file.FullName.ToLower().StartsWith(Global.StaticDirectory.ToLower()))
                        {
                            return false;
                        }
                    }
                    catch (IOException e)
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        private string GetContentType(string filePath)
        {
            foreach(string ext in acceptedExtensions.AllKeys)
            {
                if (filePath.EndsWith(ext))
                {
                    return acceptedExtensions[ext];
                }
            }
            return "application/octet-stream";
        }
    }
}