using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace RestLibrary
{
    public interface IRestRequest
    {
        /// <summary>
        ///     Adds body to request in raw string format - string should be a valid JSON.
        ///     <param name="requestRawStringBody">Raw String to add to request</param>
        /// </summary>
        IRestRequest WithRawStringBody(string requestRawStringBody);

        /// <summary>
        ///     Adds body to request in JSON format - string should be a valid JSON.
        ///     <param name="requestBody">String containing JSON to add to request</param>
        /// </summary>
        IRestRequest WithJsonBody(string requestBody);

        /// <summary>
        ///     Adds body to request in JSON format - given object should be a serializable to JSON.
        ///     <param name="requestBody">Object, that will be serialized to JSON using Newtonsoft JSON library</param>
        /// </summary>
        IRestRequest WithJsonBody(object requestBody);

        /// <summary>
        ///     Adds key/value pair to HTTP headers.
        ///     <param name="key">HTTP header key</param>
        ///     <param name="value">HTTP header value</param>
        /// </summary>
        IRestRequest WithHeaderData(string key, string value);

        /// <summary>
        ///     Adds key/value pair to HTTP headers.
        ///     <param name="key">HTTP header key</param>
        ///     <param name="value">HTTP header values</param>
        /// </summary>
        IRestRequest WithHeaderData(string key, IEnumerable<string> value);

        /// <summary>
        ///     Adds multiple key/value pairs to HTTP headers.
        ///     <param name="headerData">Pairs of HTTP header keys/values</param>
        /// </summary>
        IRestRequest WithHeaderData(Dictionary<string, string> headerData);

        /// <summary>
        ///     Adds given correlation ID to HTTP headers.
        ///     <param name="correlationId">Correlation ID to add</param>
        /// </summary>
        IRestRequest WithCorrelationId(Guid correlationId);

        /// <summary>
        ///     Invokes the request with current settings asynchronously.
        ///     Use this overload, when you want just a string result with no deserialization involved.
        ///     <returns>RestResponse structure with string as result (raw content).</returns>
        /// </summary>
        Task<RestResponse<string>> InvokeAsync();

        /// <summary>
        ///     Invokes the request with current settings asynchronously.
        ///     Use this overload, when you want deserialized JSON object as result.
        ///     <typeparam name="TResponseType">Deserialized object type</typeparam>
        ///     <returns>RestResponse structure with deserialized object as result.</returns>
        /// </summary>
        Task<RestResponse<TResponseType>> InvokeAsync<TResponseType>(); // where TResponseType : class;
    }
}