using System;

namespace Blog.Application.Models
{
    public class AccessTokenInfo
    {
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
    }
}
