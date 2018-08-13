using System.Web;
using Newtonsoft.Json.Linq;

namespace AssertOwnership
{
    public class RequestGroupContent : IHttpHandler
    {
        private OwnershipHelper helper = new OwnershipHelper();

        public bool IsReusable { get { return true; } }

        public void ProcessRequest(HttpContext context)
        {
            /* ProcessRequest is automatically called by IIS when it receives a
               request to the url pointed to by web.config */

            // Get the username from the identity of the request (which was set by PKIAuthenticationModule)
            string user = context.User.Identity.Name;
            // Get the info for the user
            JObject userInfo = helper.GetUserInfo(user);
            // If the user doesn't exist, exit with 403 response (Forbidden)
            if (userInfo["error"] != null || ((string)userInfo["level"]) != "2")
            {
                context.Response.StatusCode = 403;
                return;
            }

            JObject items = GetGroupItems(userInfo);
            context.Response.ContentType = "application/json";
            context.Response.Write(helper.JsonToString(items));
        }


        public JObject GetGroupItems(JObject userInfo)
        {
            // Get information on all items in the user's groups
            JObject items = new JObject();

            // for each group, add all items in that group to a list
            foreach (JToken group in userInfo["groups"])
            {
                JObject groupContent = helper.GetGroupContent((string)group["id"]);

                foreach (JToken item in groupContent["items"])
                {
                    if (!items.ContainsKey((string)item["id"]))
                    {
                        items[(string)item["id"]] = item;
                    }
                }
            }

            return items;
        }
    }
}