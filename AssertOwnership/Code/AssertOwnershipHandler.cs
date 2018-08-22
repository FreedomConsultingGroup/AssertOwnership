using System.Text.RegularExpressions;
using System.Web;

namespace FCG.AssertOwnership
{
    public class AssertOwnershipController : IHttpHandler
    {
        // Base controller, called by IIS

        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            /* ProcessRequest is automatically called by IIS when it receives a
               request to the url pointed to by web.config */

            string[] path = null;
            int index = 0;
            Match match = Regex.Match(context.Request.Path, @"(?:ownership)((?:\/[^\/]+)+)(?:\/*\?)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                path = match.Groups[1].Value.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                // Throw 404 exception
            }

            AOController controller;
            if (path[index] == RestContentController.Path)
            {
                controller = new RestContentController();
                controller.Defer(context, path, ++index);
                return;
            }
            else if (path[index] == StaticContentController.Path)
            {
                controller = new StaticContentController();
                controller.Defer(context, path, ++index);
                return;
            }
            else
            {
                // Throw 404 Exception
            }
        }
    }
}