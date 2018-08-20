using System.Web;

namespace FCG.AssertOwnership
{
    public class RestContentController : AOController
    {
        public static string Path { get { return "rest"; } }

        public void Defer(HttpContext context, string[] path, int index)
        {
            AOHttpHandler handler;
            if (path[1] == ChangeOwnerHandler.Path)
            {
                handler = new ChangeOwnerHandler();
                handler.ProcessRequest(context);
                return;
            }
            else if (path[1] == RequestUserContentHandler.Path)
            {
                handler = new RequestUserContentHandler();
                handler.ProcessRequest(context);
                return;
            }
            else if (path[1] == RequestGroupContentHandler.Path)
            {
                handler = new RequestGroupContentHandler();
                handler.ProcessRequest(context);
                return;
            }
        }
    }
}