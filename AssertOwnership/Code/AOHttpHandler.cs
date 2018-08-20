using System.Web;

namespace FCG.AssertOwnership
{
    public interface AOHttpHandler
    {
        void ProcessRequest(HttpContext context);
    }
}