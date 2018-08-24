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

            // parse out the path from the request
            Match match = Regex.Match(context.Request.Path, @"(?:ownership)((?:\/[^\/?]+)+)(?:\/*\?*)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                path = match.Groups[1].Value.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                context.Response.StatusCode = 404;
                return;
            }

            // Check path for either rest or static, or 404 if neither matches
            AOController controller;
            if (path[index].ToLower() == RestContentController.Path.ToLower())
            {
                controller = new RestContentController();
                controller.Defer(context, path, ++index);
                return;
            }
            else if (path[index].ToLower() == StaticContentController.Path.ToLower())
            {
                controller = new StaticContentController();
                controller.Defer(context, path, ++index);
                return;
            }
            else
            {
                context.Response.StatusCode = 404;
                return;
            }
        }
    }
}