using System.Web;
using Newtonsoft.Json.Linq;


namespace AssertOwnership
{
    public class RequestUserContentHandler : IHttpHandler
    {

        /* Set the base url for the portal and get path of certificate */
        private readonly string portalUrl = "https://fcg-arcgis-srv.freedom.local/portal/";
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

            // Get the username and starting folder from the request parameters
            HttpRequest request = context.Request;
            string username = request.QueryString["username"];
            string folder = request.QueryString["folder"];
            // If a username is not specified, default to the user who made the request
            if (username == null)
            {
                username = user;
                return;
            }
            // If a folder is not specified, default to the root folder
            if (folder == null)
            {
                folder = "/";
            }

            JObject items = GetItems(username, folder, true);
            context.Response.Write(helper.JsonToString(items));
        }

        public JObject GetUserContent(string username, string folder)
        {
            // Get user content for a specified folder
            return helper.StringToJson(helper.GetRequest("sharing/rest/content/users/" + username + "/" + folder,
                                 new string[] { "f" },
                                 new string[] { "json" }));
        }

        public JObject GetItems(string username, string folder, bool recursive)
        {
            // Get information on all items from GetUserContent
            JObject items = new JObject();
            JObject userContent = GetUserContent(username, folder);

            foreach (JToken item in userContent["items"])
            {
                items[item["id"]] = helper.GetItemInfo((string)item["id"]);
            }

            // Set recursive = false if you only want items from the specified folder and nothing else
            if (recursive)
            {
                foreach (JToken innerFolder in userContent["folders"])
                    items.AddAfterSelf(GetItems(username, (string)innerFolder, true));
            }

            return items;
        }
    }
}