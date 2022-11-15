using System.Threading.Tasks;

namespace RestLibrary
{
    public interface IRestRequest
    {
        /// <summary>
        ///     Adds key/value pair to HTTP headers.
        ///     <param name="key">HTTP header key</param>
        ///     <param name="value">HTTP header value</param>
        /// </summary>
        IRestRequest WithHeaderData(string key, string value);
        
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