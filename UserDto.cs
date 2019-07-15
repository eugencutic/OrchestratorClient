using System;
using System.Collections.Generic;
using System.Text;

namespace OrchestratorClient
{
    class UserDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Domain { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime LastLoginTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationTime { get; set; }
        public string AuthenticationSource { get; set; }
        public string Password { get; set; }
        public List<UserRoleDto> UserRoles { get; set; }
        public List<string> RolesList { get; set; }
        public List<string> LoginProviders { get; set; }
        public UserNotificationSubscription NotificationSubscription { get; set; }
        public int Id { get; set; }
    }
}
