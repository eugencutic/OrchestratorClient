using CommandLine;
using System;
using System.Net.Http;
using System.Threading;

namespace OrchestratorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
               {
                   var user = new OrchestratorClient(client)
                        .WithBasicAuthentication(o.TenancyName, o.Username, o.Username)
                        .WithBaseUrl(o.BaseUrl)
                        .Get<UserDto>(new Uri("/odata/Users", UriKind.Relative), new CancellationToken());
               });
        }
    }
}
