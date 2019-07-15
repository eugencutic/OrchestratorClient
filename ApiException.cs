using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace OrchestratorClient
{
    class ApiException: Exception
    {
        public int StatusCode { get; set; }
        public StringContent Content { get; set; }
    }
}
