using CommandLine;
using System;

namespace OrchestratorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
               {
                   Console.WriteLine(o.BaseUrl);
                   Console.WriteLine(o.TenantName);
                   Console.WriteLine(o.Username);
                   Console.WriteLine(o.Password);
               });
        }
    }
}
