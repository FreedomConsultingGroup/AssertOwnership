using System.Text.RegularExpressions;
using System.Web;

namespace FCG.AssertOwnership
{
    public class AssertOwnershipHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            /* ProcessRequest is automatically called by IIS when it receives a
               request to the url pointed to by web.config */

            string[] path = null;

            Match match = Regex.Match(context.Request.Path, @"(?:ownership)((?:\/[^\/]+)+)", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                path = match.Groups[1].Value.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                // Throw 404 exception
            }

            if (path[0] == "assert")
            {
                IHttpHandler handler = new ChangeOwnerHandler();
                handler.ProcessRequest(context);
                return;
            }
            else if (path[0] == "user")
            {
                IHttpHandler handler = new RequestUserContentHandler();
                handler.ProcessRequest(context);
                return;
            }
            else if (path[0] == "group")
            {
                IHttpHandler handler = new RequestGroupContentHandler();
                handler.ProcessRequest(context);
                return;
            }
            else
            {
                // Throw 404 Exception
            }
        }
    }
}