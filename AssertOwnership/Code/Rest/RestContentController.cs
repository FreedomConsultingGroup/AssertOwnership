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

            AOHttpHandler handler;
            if (path[index].ToLower() == ChangeOwnerHandler.Path.ToLower())
            {
                handler = new ChangeOwnerHandler();
                handler.ProcessRequest(context);
                return;
            }
            else if (path[index].ToLower() == RequestUserContentHandler.Path.ToLower())
            {
                handler = new RequestUserContentHandler();
                handler.ProcessRequest(context);
                return;
            }
            else if (path[index].ToLower() == RequestGroupContentHandler.Path.ToLower())
            {
                handler = new RequestGroupContentHandler();
                handler.ProcessRequest(context);
                return;
            }
            else
            {
                context.Response.StatusCode = 404;
                return;
            }
        }
    }
}