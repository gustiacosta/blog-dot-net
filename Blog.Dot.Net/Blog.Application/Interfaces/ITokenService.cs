using Blog.Application.Models;
using System.Collections.Generic;

namespace Blog.Application.Interfaces
{
    public interface ITokenService
    {
        AccessTokenInfo BuildToken(string key, string issuer, string audience, string userName, List<string> roles);
        bool IsTokenValid(string key, string issuer, string audience, string token);
    }
}
