using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;


namespace FCG.AssertOwnership
{
    public class ChangeOwnerHandler : RestHttpHandler
    {
        private OwnershipHelper helper;

        public ChangeOwnerHandler() : base("chown")
        {
            helper = new OwnershipHelper();
        }

        public override void ProcessRequest(HttpContext context)
        {
            HttpRequest request = context.Request;
            if (request.HttpMethod != "POST")
            {
                Global.LogInfo("Status: 405 returned. Invalid request method. required POST, received " + request.HttpMethod);
                context.Response.StatusCode = 405;
                return;
            }
            // Get the username from the identity of the request (which was set by PKIAuthenticationModule)
            string user = context.User.Identity.Name;
            // Get the info for the user
            JObject userInfo = helper.GetUserInfo(user);
            // If the user doesn't exist or doesn't have the right permissions, exit with 403 response (Forbidden)
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

            // Get item ID, the new owner of the item, and the destination folder from the request parameters
            string itemID = request["id"];
            string newOwner = request["newowner"];
            string newFolder = request["newfolder"];

            // If the item ID isn't specified, exit with 400 response (Client Error)
            if (itemID == null)
            {
                Global.LogInfo("Status: 400 returned. User did not specify item ID");
                context.Response.StatusCode = 400;
                return;
            }

            // If the new owner is not specified, default to the user who made the request
            if (newOwner == null)
            {
                newOwner = user;
            }

            // If the destination folder is not specified, default to "/"
            if (newFolder == null)
            {
                newFolder = "/";
            }

            // If the new owner is different from the user making the request, the user must be an admin with reassignItems privileges
            // This means that regular users can only assign items to themselves
            if (newOwner != user)
            {
                if (!userInfo["privileges"].Contains("portal:admin:reassignItems"))
                {
                    context.Response.StatusCode = 403;
                    Global.LogInfo("Status: 403 returned. Specified user does not have portal:admin:reassignItems permission, which is required to assert ownership as another user");
                    return;
                }
            }

            JObject itemInfo = helper.GetItemInfo(itemID);
            if (itemInfo["ownerFolder"] == null)
            {
                itemInfo["ownerFolder"] = "/";
            }
            string oldOwner = (string)itemInfo["owner"];

            if (oldOwner == newOwner)
            {
                context.Response.StatusCode = 400;
                Global.LogInfo("Status: 400 returned. Specified user " + user + " already owns item " + itemID);
                return;
            }
            /* Check to see if the item, the current owner, and the new owner all share a group.
               If not, then exit with 401 response (Unauthorized) */
            if (InvalidGroups(itemInfo, newOwner))
            {
                context.Response.StatusCode = 401;
                Global.LogInfo("Status: 401 returned. User is unauthorized to take ownership of this item, some groups not shared between item, old owner, and new owner");
                return;
            }

            // Generate a token to use with API resuests
            string token = helper.GenerateToken();

            JObject response = helper.DeserializeJson<JObject>(helper.Request(Global.PortalUrl + "/sharing/rest/content/users/" + itemInfo["owner"] + "/" + itemInfo["ownerFolder"] + "/items/" + itemID + "/reassign",
                                         new string[] { "targetUsername", "targetFolderName", "token", "f" },
                                         new string[] { newOwner, newFolder, token, "json" },
                                         "POST").Result);

            // Return success if it correectly transfered ownership. Otherwise, return the error message
            context.Response.Write(response);
            if (response.Value<bool>("success") == true)
            {
                Global.LogInfo("Status: 200 returned. Item with id " + itemID + " successfully transfered from " + oldOwner + " to " + newOwner);
            }
            return;
        }


        private bool InvalidGroups(JObject itemInfo, string newOwner)
        {
            /* Checks which group(s) the old owner and the item share, then check to see if the new owner is in those group(s) as well. */
            JToken itemGroupInfo = itemInfo["groups"];

            JObject oldUserInfo = helper.GetUserInfo((string)itemInfo["owner"]);
            JObject newUserInfo = helper.GetUserInfo(newOwner);

            bool noMatch = true;
            string[] matchingGroup = { };
            // Add matching groups to the groups to check in the new owner
            foreach (JToken itemGroup in itemGroupInfo)
            {
                if (Global.GroupWhitelist.Contains((string)itemGroup["id"]))
                {
                    continue;
                }
                noMatch = true;
                foreach (JToken newUserGroup in newUserInfo["groups"])
                {
                    if (newUserGroup["id"] != null && (string)itemGroup["id"] == (string)newUserGroup["id"])
                    {
                        noMatch = false;
                        break;
                    }
                }
                if (noMatch)
                {
                    return true;
                }
            }

            return false;
        }
    }



}