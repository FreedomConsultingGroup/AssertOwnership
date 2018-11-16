using Newtonsoft.Json.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public class WhoAmIHandler : APIHttpHandler
    {
        public WhoAmIHandler() : base("whoami") { }

        public override void ProcessRequest(HttpContext context)
        {
            // return the username of the current user
            string name = context.User.Identity.Name;
            OwnershipHelper helper = OwnershipHelper.getInstance();

            JObject userInfo = helper.GetUserInfo(name);
            string thumbnail = (string)userInfo["thumbnail"];

            context.Response.ContentType = "application/json";
            if (thumbnail == null)
            {
                context.Response.Write("{\"name\": \"" + name + "\", \"thumbnail\": " + thumbnail + " }");
            }
            else
            {
                context.Response.Write("{\"name\": \"" + name + "\", \"thumbnail\": \"" + thumbnail + "\" }");
            }

            Global.LogInfo("User " + name + " requested whoami.");
            return;
        }
    }
}