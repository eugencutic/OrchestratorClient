using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrchestratorClient
{
    class OrchestratorClient: IFluentHttpClient<OrchestratorClient>
    {
        private readonly HttpClient _client;
        private Dictionary<string, string> Headers;
        private string TenancyName;
        private string Username;
        private string Password;
        private int OrganzationUnitId;
        private Uri BaseUrl = new Uri("http://localhost:6234");
        private readonly Uri _loginUrl = new Uri("/api/Account/Authenticate", UriKind.Relative);

        private class AuthenticationObject
        {
            [JsonProperty(PropertyName = "tenancyName")]
            public string TenancyName { get; set; }
            [JsonProperty(PropertyName = "usernameOrEmailAddress")]
            public string Username { get; set; }
            [JsonProperty(PropertyName = "password")]
            public string Password { get; set; }
        }

        public OrchestratorClient(HttpClient client)
        {
            _client = client;
        }
        public OrchestratorClient WithHeaders(Dictionary<string, string> headers)
        {
            this.Headers = headers;
            return this;
        }

        public OrchestratorClient WithBasicAuthentication(string tenantName, string username, string password)
        {
            TenancyName = tenantName;
            Username = username;
            Password = password;
            return this;
        }

        public OrchestratorClient WithBaseUrl(Uri baseUrl)
        {
            this.BaseUrl = baseUrl;
            return this;
        }

        public OrchestratorClient WithOrganizationUnitId(int organzationUnitId)
        {
            OrganzationUnitId = organzationUnitId;
            return this;
        }

        public async Task<T> Get<T>(Uri url, CancellationToken ct = default) where T : class
        {
            return await RequestAsync<T>(url, HttpMethod.Get, null, Headers, ct);
        }

        public async Task<List<T>> GetList<T>(Uri url, CancellationToken ct = default) where T : class
        {
            return await RequestAsync<List<T>>(url, HttpMethod.Get, null, Headers, ct);
        }

        public async Task<TResponse> Post<TRequest, TResponse>(Uri url, TRequest body,
            CancellationToken ct = default)
            where TResponse : class
            where TRequest : class
        {
            using (var content = SerializeContent<TRequest>(body))
            {
                return await RequestAsync<TResponse>(url, HttpMethod.Post, content, Headers);
            }
        }

        protected async Task<T> RequestAsync<T>(Uri serviceUrl, HttpMethod method, HttpContent content, Dictionary<string, string> headers = null, CancellationToken ct = default)
            where T : class
        {
            var response = await RequestAsync(serviceUrl, method, content, headers, ct);

            return await ReadBodyAndDeserialize<T>(response);
        }

        protected async Task<HttpResponseMessage> RequestAsync(Uri serviceUrl, HttpMethod method, HttpContent content, Dictionary<string, string> headers = null, CancellationToken ct = default, bool retryLogin = true)
        {
            var response = await CreateAndSendMessage(serviceUrl, method, content, headers, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (retryLogin)
                {
                    if (headers == null)
                        headers = new Dictionary<string, string>();
                    var token = await GetAccessToken(ct);
                    headers["Authorization"] = "Bearer " + token;
                    return await RequestAsync(serviceUrl, method, content, headers, ct, false);
                }
                else
                {
                    throw new Exception("Authentication failed");
                }
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException { StatusCode = (int)response.StatusCode, Content = (StringContent)content };
            }

            return response;
        }

        protected async Task<string> GetAccessToken(CancellationToken ct)
        {
            using (var content = SerializeContent<AuthenticationObject>(
                new AuthenticationObject
                {
                    TenancyName = this.TenancyName,
                    Username = this.Username,
                    Password = this.Password
                }))
            {
                var response = await CreateAndSendMessage(_loginUrl, HttpMethod.Post, content, Headers, ct);
                var apiResponse = await ReadBodyAndDeserialize<ApiResponse>(response);
                return apiResponse.Result;
            }
        }

        protected static StringContent SerializeContent<T>(T payload)
        {
            var payloadJson = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(payloadJson);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return stringContent;
        }

        private async Task<HttpResponseMessage> CreateAndSendMessage(Uri url, HttpMethod method,
           HttpContent content, Dictionary<string, string> headers = null,
           CancellationToken ct = default)
        {
            using (var message = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(BaseUrl, url),
                Content = content,
            })
            {
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        message.Headers.Add(item.Key, item.Value);
                    }
                }
                return await _client.SendAsync(message, ct);
            }
        }

        private async Task<T> ReadBodyAndDeserialize<T>(HttpResponseMessage responseMessage) where T : class
        {
            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            return string.IsNullOrEmpty(responseBody) ? null : JsonConvert.DeserializeObject<T>(responseBody, jsonSerializerSettings);
        }
    }
}
