using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrchestratorClient
{
    class Options
    {
        [Option("url")]
        public Uri BaseUrl { get; set; }

        [Option('t')]
        public string TenantName { get; set; }

        [Option('u')]
        public string Username { get; set; }

        [Option('p')]
        public string Password { get; set; }
    }
}
