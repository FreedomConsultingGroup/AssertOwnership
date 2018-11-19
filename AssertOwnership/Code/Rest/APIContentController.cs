using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public class APIContentController : AOController
    {
        public static string Path { get { return "rest"; } }

        public void Defer(HttpContext context, string[] path, int index)
        {
            /* Called by AssertOwnershipController, points to the handlers for the rest API
               based on the path of the request*/

            // TODO Change this to different design pattern for sanity
            foreach (APIHttpHandler handler in Global.APIHandlers)
            {
                if (path[index].ToLower() == handler.Path.ToLower())
                {
                    handler.ProcessRequest(context);
                    return;
                }
            }
            context.Response.StatusCode = 404;
            Global.LogInfo("Status: 404 returned. Requested URI path does not exist");
            return;
        }
    }
}