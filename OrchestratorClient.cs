using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
            return await RequestAsync<T, T>(url, HttpMethod.Get, null, ct);
        }

        public async Task<List<T>> GetList<T>(Uri url, CancellationToken ct = default) where T : class
        {
            return await RequestAsync<List<T>, T>(url, HttpMethod.Get, null, ct);
        }

        public async Task<TResponse> Post<TRequest, TResponse>(Uri url, TRequest body,
            CancellationToken ct = default)
            where TResponse : class
            where TRequest : class
        {
            
            return await RequestAsync<TResponse, TRequest>(url, HttpMethod.Post, body, ct);
            
        }

        public async Task<HttpResponseMessage> UploadPackage(string path, CancellationToken ct = default)
        {
            var content = SerializePackageContent(path);
            var response = await CreateAndSendMessage(new Uri("/odata/Processes/UiPath.Server.Configuration.OData.UploadPackage", UriKind.Relative), HttpMethod.Post, content, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (Headers == null)
                    Headers = new Dictionary<string, string>();
                var token = await GetAccessToken(ct);
                Headers["Authorization"] = "Bearer " + token;
                content = SerializePackageContent(path);
                response = await CreateAndSendMessage(new Uri("/odata/Processes/UiPath.Server.Configuration.OData.UploadPackage", UriKind.Relative), HttpMethod.Post, content, ct);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("Authentication failed");
            }

            if (!response.IsSuccessStatusCode)
                throw new ApiException { Content = (StringContent)response.Content, StatusCode = (int)response.StatusCode };

            return response;
        }

        public MultipartFormDataContent SerializePackageContent(string path)
        {
            try
            {
                using (var package = File.OpenRead(path))
                {
                    var streamContent = new StreamContent(File.OpenRead(path));
                    var multipartContent = new MultipartFormDataContent();
                    multipartContent.Add(streamContent, "file", path);
                    return multipartContent;
                }
            }
            catch(IOException)
            {
                Console.WriteLine("File not found");
            }
            return null;
        }

        protected async Task<T> RequestAsync<T, TRequest>(Uri serviceUrl, HttpMethod method, TRequest body, CancellationToken ct = default)
            where T : class
        {
            Func<Uri, HttpMethod, HttpContent, CancellationToken, Task<HttpResponseMessage>>
                request = RequestAsync;
            var response = await RequestAndRetry(() => RequestAsync<TRequest>(serviceUrl, method, body, ct), ct);
            //var response = await RequestAsync(serviceUrl, method, content, headers, ct);

            return await ReadBodyAndDeserialize<T>(response);
        }

        protected async Task<HttpResponseMessage> RequestAndRetry(Func<Task<HttpResponseMessage>> request, CancellationToken ct)
        {
            
            var response = await request();
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (Headers == null)
                    Headers = new Dictionary<string, string>();
                var token = await GetAccessToken(ct);
                Headers["Authorization"] = "Bearer " + token;
                response = await request();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("Authentication failed.");
            }
            return response;
        }

        protected async Task<HttpResponseMessage> RequestAsync<TRequest>(Uri serviceUrl, HttpMethod method, TRequest body, CancellationToken ct = default)//, bool retryLogin = true)
        {
            using (var content = SerializeContent<TRequest>(body))
            {
                
                var response = await CreateAndSendMessage(serviceUrl, method, content, ct);

                if (!response.IsSuccessStatusCode && !(response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                {
                    throw new ApiException { StatusCode = (int)response.StatusCode, Content = (StringContent)content };
                }
                return response;
            }
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
                var response = await CreateAndSendMessage(_loginUrl, HttpMethod.Post, content, ct);
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

        private async Task<HttpResponseMessage> CreateAndSendMessage(Uri url, HttpMethod method, HttpContent content,CancellationToken ct = default)
        {
            using (var message = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(BaseUrl, url),
                Content = content,
            })
            {
                if (Headers != null)
                {
                    foreach (var item in Headers)
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
