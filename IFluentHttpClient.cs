using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrchestratorClient
{
    interface IFluentHttpClient<T>
    {
        IFluentHttpClient<T> WithHeaders(Dictionary<string, string> headers);
        IFluentHttpClient<T> WithBasicAuthentication(string tenantName, string username, string password);
        IFluentHttpClient<T> WithBaseUrl(Uri baseUrl);

        Task<T> Get<T>(Uri url, CancellationToken ct) where T : class;
        Task<List<T>> GetList<T>(Uri url, CancellationToken ct) where T : class;
        Task<TResponse> Post<TRequest, TResponse>(Uri url, TRequest body, CancellationToken ct) 
            where TRequest : class 
            where TResponse : class;
    }
}
