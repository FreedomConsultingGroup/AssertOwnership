using Newtonsoft.Json.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public class WhoAmIHandler : RestHttpHandler
    {
        public WhoAmIHandler() : base("whoami") { }

        public override void ProcessRequest(HttpContext context)
        {
            // return the username of the current user
            context.Response.ContentType = "application/json";
            JObject userInfo = 
            string name = context.User.Identity.Name;
            string thumbnail = 
            context.Response.Write("{\"name\": \"" + name + "\"}");

            Global.LogInfo("User " + name + " requested whoami.");
            return;
        }
    }
}