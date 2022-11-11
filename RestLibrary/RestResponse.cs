using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System;

namespace RestLibrary
{
    public class RestResponse<TResponseResult>
    {
        /// <summary>
        ///     Content of the request, in the form of a raw string
        /// </summary>
        public string RawRequestBody { get; set; }

        /// <summary>
        ///     Collection of request headers to examine
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; }

        /// <summary>
        ///     URI invoked
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        ///     HttpMethod invoked
        /// </summary>
        public HttpMethod HttpMethod { get; set; }

        /// <summary>
        ///     Content of the response, in the form of a raw string
        /// </summary>
        public string RawResponseBody { get; set; }

        /// <summary>
        ///     Success flag
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        ///     Exception that has occured or null, if no error occured
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     HTTP status code of the response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        ///     Collection of response headers to examine
        /// </summary>
        public Dictionary<string, string> ResponseHeaders { get; set; }

        /// <summary>
        ///     Deserialized result
        /// </summary>
        public TResponseResult Result { get; set; }
    }
}