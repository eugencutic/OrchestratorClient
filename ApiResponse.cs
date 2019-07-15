using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrchestratorClient
{
    class ApiResponse
    {
        public string Result { get; set; }

        public string TargetUrl { get; set; }

        public bool Success { get; set; }

        public ErrorInfo[] Error { get; set; }

        public bool UnAuthorizedRequest { get; set; }

        [JsonProperty("__abp")]
        public bool Abp { get; }
    }
}
