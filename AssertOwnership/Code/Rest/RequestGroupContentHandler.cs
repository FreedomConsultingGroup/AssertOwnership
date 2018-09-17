using System.Web;
using Newtonsoft.Json.Linq;


namespace FCG.AssertOwnership
{
    public class RequestGroupContentHandler : RestHttpHandler
    {
        private OwnershipHelper helper;

        public RequestGroupContentHandler() : base("group")
        {
            helper = new OwnershipHelper();
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

            JObject items = GetGroupItems(userInfo);
            if (items == null)
            {
                Global.LogInfo("Status: 400 returned. Specified user does not belong to any groups");
                context.Response.StatusCode = 400;
                return;
            }
            context.Response.ContentType = "application/json";
            Global.LogInfo("Status: 200 returned. Returned group item information for user " + user);
            context.Response.Write(helper.JsonToString(items));
            return;
        }


        public JObject GetGroupItems(JObject userInfo)
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
                string groupId = (string)group["id"];
                JObject groupContent = helper.GetGroupContent(groupId);

                foreach (JToken item in groupContent["items"])
                {
                    string itemId = (string)item["id"];
                    JObject itemInfo = helper.GetItemInfo(itemId);

                    // Make sure the user shares all groups with the item
                    bool addItem = false;
                    foreach (JToken itemGroup in itemInfo["groups"])
                    {
                        addItem = false;
                        foreach (JToken userGroup in userInfo["groups"])
                        {
                            if ((string)userGroup["id"] == (string)itemGroup["id"])
                            {
                                addItem = true;
                                break;
                            }
                        }
                        if (!addItem)
                        {
                            // if no match in user groups, break because addItem is false so the item will not be added to the items list
                            break;
                        }
                    }

                    if (addItem && !items.ContainsKey(itemId))
                    {
                        items[itemId] = item;
                        items[itemId]["groups"] = helper.DeserializeJson<JArray>("[{\"title\": \"" + group["title"] + "\", \"id\": \"" + groupId + "\"}]");
                    }
                    else if (addItem)
                    {
                        items[itemId]["groups"].Last.AddAfterSelf(helper.DeserializeJson<JObject>("{\"title\": \"" + group["title"] + "\", \"id\": \"" + groupId + "\"}"));
                    }
                }
            }

            return items;
        }
    }
}