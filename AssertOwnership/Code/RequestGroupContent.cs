using System.Web;
using Newtonsoft.Json.Linq;

namespace AssertOwnership
{
    public class RequestGroupContent
    {
        private static OwnershipHelper helper = new OwnershipHelper();
        
        public static void ProcessRequest(HttpContext context)
        {

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
            if (items == null)
            {
                context.Response.StatusCode = 400;
                return;
            }
            context.Response.ContentType = "application/json";
            context.Response.Write(helper.JsonToString(items));
        }


        public static JObject GetGroupItems(JObject userInfo)
        {
            // Get information on all items in the user's groups
            JObject items = new JObject();

            if (userInfo["groups"] == null)
            {
                return null;
            }

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