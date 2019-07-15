using System;
using System.Collections.Generic;
using System.Text;

namespace OrchestratorClient
{
    class UserNotificationSubscription
    {
        public bool Queues { get; set; }
        public bool Robots { get; set; }
        public bool Jobs { get; set; }
        public bool Schedules { get; set; }
    }
}
