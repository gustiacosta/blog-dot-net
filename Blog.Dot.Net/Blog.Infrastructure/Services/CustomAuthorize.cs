using Blog.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.Extensions.Configuration;

namespace Blog.Infrastructure.Services
{
    public class CustomAuthorize : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public CustomAuthorize(ITokenService tokenService, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //context.HttpContext.Session.get
            //var ok = context.HttpContext.User.IsInRole;

            throw new NotImplementedException();
        }
    }
}
