using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestLibrary
{
    public class RestRequest : IRestRequest
    {
        private readonly HttpClient _httpClient;

        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly ILogger _logger;
        private string _requestJsonBody;

        public RestRequest(HttpClient httpClient, string relativeUrl, HttpMethod httpMethod, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(relativeUrl, UriKind.Relative),
                Method = httpMethod
            };
        }

        public IRestRequest WithHeaderData(string key, string value)
        {
            if (key == null || value == null)
            {
                return this;
            }

            _httpRequestMessage.Headers.Add(key, value);
            return this;
        }

        /// <summary>
        ///     Invokes the request with current settings asynchronously.
        /// </summary>
        public async Task<RestResponse<string>> InvokeAsync()
        {
            var result = new RestResponse<string>
            {
                Uri = _httpRequestMessage.RequestUri.ToString(),
                RawRequestBody = _requestJsonBody,
                HttpMethod = _httpRequestMessage.Method,
                RequestHeaders = _httpRequestMessage.Headers.ToDictionary(header => header.Key,
                    header => string.Join(";", header.Value))
            };

            try
            {
                var response = await _httpClient.SendAsync(_httpRequestMessage);

                result.IsSuccessful = response.IsSuccessStatusCode;
                result.StatusCode = response.StatusCode;
                result.ResponseHeaders =
                    response.Headers.ToDictionary(header => header.Key, header => string.Join(";", header.Value));
                if (response.Content != null)
                {
                    result.RawResponseBody = await response.Content.ReadAsStringAsync();
                }

                if (response.IsSuccessStatusCode)
                {
                    result.Result = result.RawResponseBody;
                }
                else
                {
                    var msg =
                        $"{_httpRequestMessage.Method} failure [{response.StatusCode}] on '{_httpRequestMessage.RequestUri}': {result.RawResponseBody}";
                    _logger.LogError(msg);

                    throw new HttpStatusCodeException(response.StatusCode, msg);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP error occured! {ex.Message}");
                result.IsSuccessful = false;
                result.Exception = ex;
            }
            catch (IOException ex)
            {
                _logger.LogError($"IO error occured! {ex.Message}");
                result.IsSuccessful = false;
                result.Exception = ex;
                result.StatusCode = HttpStatusCode.ServiceUnavailable;
            }

            return result;
        }

        public async Task<RestResponse<TResponseType>> InvokeAsync<TResponseType>() //where TResponseType : class
        {
            if (typeof(TResponseType) == typeof(string))
            {
                var stringResult = await InvokeAsync();
                return (RestResponse<TResponseType>) (object) stringResult;
            }

            var response = await InvokeAsync();
            var result = new RestResponse<TResponseType>
            {
                Uri = response.Uri,
                RawRequestBody = response.RawRequestBody,
                HttpMethod = response.HttpMethod,
                RequestHeaders = response.RequestHeaders,
                IsSuccessful = response.IsSuccessful,
                RawResponseBody = response.RawResponseBody,
                StatusCode = response.StatusCode,
                Exception = response.Exception,
                ResponseHeaders = response.ResponseHeaders
            };

            if (!result.IsSuccessful)
            {
                return result;
            }

            try
            {
                result.Result = JsonConvert.DeserializeObject<TResponseType>(result.RawResponseBody);
            }
            catch (JsonException ex) when (ex is JsonReaderException || ex is JsonSerializationException)
            {
                _logger.LogError($"Problem occured during deseralization of object! {ex.Message}");
                result.IsSuccessful = false;
                result.Exception = ex;
            }

            return result;
        }
    }
}