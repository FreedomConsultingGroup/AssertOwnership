using System.Text.RegularExpressions;
using System.Web;

namespace AssertOwnership.Code
{
    public class AssertOwnershipHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }


        public void ProcessRequest(HttpContext context)
        {
            /* ProcessRequest is automatically called by IIS when it receives a
               request to the url pointed to by web.config */

            Match match = Regex.Match(context.Request.Path, @"(?(DEFINE)(?'part'\/[^\/]+))(?:ownership)((?P>part))+");
            if (match.Success)
            {

            }
            
        }
    }
}