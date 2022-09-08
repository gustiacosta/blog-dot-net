using System;
using System.Collections.Generic;

namespace Blog.Application.Models
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        public List<string> UserRoles { get; set; } = new();
    }
}
