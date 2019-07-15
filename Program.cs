using CommandLine;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OrchestratorClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new HttpClient();
            string tenancyName = "", username = "", password = "";
            Uri baseUrl = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
               {
                   tenancyName = o.TenancyName;
                   username = o.Username;
                   password = o.Password;
                   baseUrl = o.BaseUrl;
               });
            Console.WriteLine(baseUrl);
            List <UserDto> users = await  new OrchestratorClient(client)
                          .WithBasicAuthentication(tenancyName, username, password)
                          .WithBaseUrl(baseUrl)
                          .GetList<UserDto>(new Uri("/odata/Users", UriKind.Relative), new CancellationToken());
            foreach (UserDto user in users)
            {
                Console.WriteLine(user.Name);
            }
        }
    }
}
