using Newtonsoft.Json.Linq;
using System.Net;
using System;

namespace RestLibrary
{
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ContentType { get; set; } = @"text/plain";

        public HttpStatusCodeException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCodeException(HttpStatusCode statusCode, Exception inner) :
            this(statusCode, inner.ToString())
        { }

        public HttpStatusCodeException(HttpStatusCode statusCode, JObject errorObject) : this(statusCode,
            errorObject.ToString())
        {
            ContentType = @"application/json";
        }

    }
}