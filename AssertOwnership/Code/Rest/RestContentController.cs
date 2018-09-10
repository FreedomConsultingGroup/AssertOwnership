using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public class RestContentController : AOController
    {
        public static string Path { get { return "rest"; } }

        public void Defer(HttpContext context, string[] path, int index)
        {
            /* Called by AssertOwnershipController, points to the handlers for the rest API
               based on the path of the request*/

            RestHttpHandler[] handlers = GetImplementedClasses();
            foreach (RestHttpHandler handler in handlers)
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

        private RestHttpHandler[] GetImplementedClasses()
        {
            List<RestHttpHandler> handlers = new List<RestHttpHandler>();
            // get assemblies used in the current domain
            foreach (var domain in AppDomain.CurrentDomain.GetAssemblies())
            {
                // get all types in each assembly
                foreach (Type t in domain.GetExportedTypes())
                {
                    // check if it inherits from RestHttpHandler
                    if (t.IsSubclassOf(typeof(RestHttpHandler)) && !t.IsAbstract)
                    {
                        // initialize the object and add it to the list
                        handlers.Add((RestHttpHandler)Activator.CreateInstance(t));
                    }
                }
            }
            return handlers.ToArray();
        }
    }
}