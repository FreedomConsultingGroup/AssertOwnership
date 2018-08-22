using System;
using System.Web;

namespace FCG.AssertOwnership
{
    public class HttpException
    {
        public static void SendHttpException(HttpContext context, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
        }
    }
}