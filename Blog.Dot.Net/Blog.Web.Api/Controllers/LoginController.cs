using AutoMapper;
using Blog.Application.Interfaces;
using Blog.Application.Models;
using Blog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Web.Api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginController(ILogger<LoginController> logger, ITokenService tokenService, IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _config = config;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RequestResponse>> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // check if the user with the same email exist
                try
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.UserName.Trim());

                    if (existingUser == null)
                    {
                        // We dont want to give to much information on why the request has failed for security reasons
                        return BadRequest(new RequestResponse()
                        {
                            IsSuccess = false,
                            Errors = new List<string>()
                            {
                                "Invalid authentication request"
                            }
                        });
                    }

                    var userRoles = await _userManager.GetRolesAsync(existingUser);

                    // Now we need to check if the user has entered the right password
                    var isCorrect = await _userManager.CheckPasswordAsync(existingUser, model.Password);

                    if (isCorrect)
                    {
                        var jwtTokenInfo = _tokenService.BuildToken(_config["Jwt:Key"], _config["Jwt:Issuer"], _config["Jwt:Audience"], model.UserName, userRoles.ToList());

                        return Ok(new
                        {
                            IsSuccess = true,
                            UserName = model.UserName,
                            AccessToken = jwtTokenInfo.AccessToken,
                            Expires = jwtTokenInfo.Expires,
                            UserRoles = userRoles.ToList()
                        });
                    }

                    // We dont want to give to much information on why the request has failed for security reasons
                    return BadRequest(new RequestResponse()
                    {
                        IsSuccess = false,
                        Errors = new List<string>()
                        {
                            "Invalid authentication request"
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            return BadRequest(new RequestResponse()
            {
                IsSuccess = false,
                Errors = new List<string>()
                {
                    "Invalid payload"
                }
            });
        }
    }
}
