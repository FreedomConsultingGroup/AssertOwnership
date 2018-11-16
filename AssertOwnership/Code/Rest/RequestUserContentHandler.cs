using System.Web;
using Newtonsoft.Json.Linq;


namespace FCG.AssertOwnership
{
    public class RequestUserContentHandler : APIHttpHandler
    {
        private OwnershipHelper helper;

        public RequestUserContentHandler() : base("user")
        {
            helper = OwnershipHelper.getInstance();
        }

        public override void ProcessRequest(HttpContext context)
        {

            // Get the username from the identity of the request (which was set by PKIAuthenticationModule)
            string user = context.User.Identity.Name;
            // Get the info for the user
            JObject userInfo = helper.GetUserInfo(user);
            // If the user doesn't exist, exit with 403 response (Forbidden)
            if (userInfo["error"] != null || ((string)userInfo["level"]) != "2")
            {
                if (userInfo["error"] != null)
                {
                    Global.LogInfo("Status: 403 returned. Specified user does not exist");
                }
                else
                {
                    Global.LogInfo("Status: 403 returned. Specified user does not have the correct permissions for this request");
                }
                context.Response.StatusCode = 403;
                return;
            }

            // Get the username and starting folder from the request parameters
            HttpRequest request = context.Request;
            string username = request.QueryString["username"];
            string folder = request.QueryString["folder"];
            // If a username is not specified, default to the user who made the request
            if (username == null)
            {
                username = user;
            }
            // If a folder is not specified, default to the root folder
            if (folder == null)
            {
                folder = "";
            }

            JObject items = GetItems(username, folder, true);
            context.Response.ContentType = "application/json";
            Global.LogInfo("Status: 200 returned. Returned user item information for user " + user);
            context.Response.Write(helper.JsonToString(items));
        }


        public JObject GetItems(string username, string folder, bool recursive)
        {
            // Get information on all items from GetUserContent
            JObject items = new JObject();
            JObject userContent = helper.GetUserContent(username, folder);

            foreach (JToken item in userContent["items"])
            {
                items[(string)item["id"]] = helper.GetItemInfo((string)item["id"]);
            }

            // Set recursive = false if you only want items from the specified folder and nothing else
            if (recursive && userContent.ContainsKey("folders"))
            {
                foreach (JToken innerFolder in userContent["folders"])
                    items[(string)innerFolder["id"]] = GetItems(username, (string)innerFolder["id"], true);
            }

            return items;
        }
    }
}