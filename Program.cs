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

            HttpResponseMessage response = await new OrchestratorClient(client)
                                .WithBasicAuthentication(tenancyName, username, password)
                                .WithOrganizationUnitId(1)
                                .WithBaseUrl(baseUrl)
                                .UploadPackage(@"C:\Users\eugen.cutic\UiPath Studio Packages\Lesson-2-Practice-1.1.0.1.nupkg");

            //ODataResponse users = await  new OrchestratorClient(client)
            //              .WithBasicAuthentication(tenancyName, username, password)
            //              .WithOrganizationUnitId(3)
            //              .WithBaseUrl(baseUrl)
            //              .Get<ODataResponse>(new Uri("/odata/Users", UriKind.Relative), new CancellationToken());
            //foreach(var user in users.value)
            //    Console.WriteLine(user.UserName);
            
        }
    }
}
