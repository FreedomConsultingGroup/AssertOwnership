using System;

namespace FCG.AssertOwnership
{
    public class HttpException : Exception
    {
        private int statusCode;

        public HttpException(string message, int statusCode) : base(message)
        {
            this.statusCode = statusCode;
        }

        public int StatusCode { get { return this.statusCode; } }
    }
}