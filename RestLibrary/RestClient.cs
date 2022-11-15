using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestLibrary
{
    public class RestClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestClient> _logger;

        public RestClient(HttpClient httpClient, ILogger<RestClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private IRestRequest CreateRequest(string endpointUrl, HttpMethod httpMethod)
        {
            return new RestRequest(_httpClient, endpointUrl, httpMethod, _logger);
        }

        public async Task<T> CallAsync<T>(string url, HttpMethod method, Action<IRestRequest> additionalAction = null)
        {
            var request = CreateRequest(url, method);
            additionalAction?.Invoke(request);
            var response = await request.InvokeAsync<T>();

            if (!response.IsSuccessful)
            {
                throw response.Exception ?? new ArgumentNullException(nameof(response.Exception));
            }

            return response.Result;
        }

        public async Task CallAsync(string url, HttpMethod method, Action<IRestRequest> additionalAction = null)
        {
            var request = CreateRequest(url, method);
            additionalAction?.Invoke(request);
            var response = await request.InvokeAsync();
            if (!response.IsSuccessful)
            {
                throw response.Exception ?? new ArgumentNullException(nameof(response.Exception));
            }
        }
    }
}