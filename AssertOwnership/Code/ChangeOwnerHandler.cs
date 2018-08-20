using System.Web;
using System.Linq;
using Newtonsoft.Json.Linq;


namespace FCG.AssertOwnership
{
    public class ChangeOwnerHandler : IHttpHandler
    {
        public bool IsReusable { get { return true; } }

        private OwnershipHelper helper = new OwnershipHelper();

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            if (request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                return;
            }
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

            // Get item ID, the new owner of the item, and the destination folder from the request parameters
            string itemID = request["itemid"];
            string newOwner = request["newowner"];
            string newFolder = request["newfolder"];

            // If the item ID or new owner aren't specified exit with 400 response (Client Error)
            if (itemID == null || newOwner == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            // If the destination folder is not specified, default to "/"
            if (newFolder == null)
            {
                newFolder = "";
            }

            JObject itemInfo = helper.GetItemInfo(itemID);
            if (itemInfo["ownerFolder"] == null)
            {
                itemInfo["ownerFolder"] = "";
            }

            /* Check to see if the item, the current owner, and the new owner all share a group.
               If not, then exit with 401 response (Unauthorized) */
            if (InvalidGroups(itemInfo, newOwner))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Generate a token to use with API resuests
            string token = helper.GenerateToken();

            JObject response = helper.StringToJson(helper.GetRequest(helper.portalUrl + "/sharing/rest/content/users/" + itemInfo["owner"] + "/" + itemInfo["ownerFolder"] + "/items/" + itemID + "/reassign",
                                         new string[] { "targetUsername", "targetFoldername", "token", "f" },
                                         new string[] { newOwner, newFolder, token, "json" }));

            // Return the word success if it correectly transfered ownership. Otherwise, return the error message
            if (response["success"] != null)
            {
                context.Response.Write("success");
                return;
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.Write((string)response["error"]);
                return;
            }
        }


        private bool InvalidGroups(JObject itemInfo, string newOwner)
        {
            /* Checks which group(s) the old owner and the item share, then check to see if the new owner is in those group(s) as well. */
            JToken itemGroupInfo = itemInfo["groups"]["member"];
            JObject oldUserInfo = helper.GetUserInfo((string)itemInfo["owner"]);
            JObject newUserInfo = helper.GetUserInfo(newOwner);

            bool matches = false;
            string[] matchingGroup = { };
            // Add matching groups to the groups to check in the new owner
            foreach (JToken itemGroup in itemGroupInfo)
            {
                foreach (JToken oldUserGroup in oldUserInfo["groups"])
                {
                    if (oldUserGroup["id"] != null && itemGroup["id"] == oldUserGroup["id"])
                    {
                        matchingGroup[matchingGroup.Length] = (string)itemGroup["id"];
                        break;
                    }
                }
            }

            if (matchingGroup.Length == 0) { return false; }

            // Check new owner for any of the matching groups
            foreach (JToken newUserGroup in newUserInfo["groups"])
            {
                if (matchingGroup.Contains((string)newUserGroup["id"]))
                {
                    matches = true;
                    break;
                }
            }

            return matches;
        }
    }


    
}