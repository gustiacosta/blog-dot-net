using Blog.Application.Models;
using Blog.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Portal.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager; //updated from IdentityUser
        private readonly SignInManager<ApplicationUser> _signInManager; //updated from IdentityUser
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                LoginResponse loginResponse;

                var loginModel = new
                {
                    UserName = Input.Email,
                    Input.Password,
                };
                var json = JsonConvert.SerializeObject(loginModel);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = _httpClientFactory.CreateClient("Default");

                using (var response = await httpClient.PostAsync("login", data))
                {
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();

                    loginResponse = JsonConvert.DeserializeObject<LoginResponse>(content);
                }

                if (loginResponse == null || !loginResponse.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                HttpContext.Session.SetString("UserName", loginModel.UserName);
                HttpContext.Session.SetString("Token", loginResponse.AccessToken);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, loginModel.UserName));
                foreach (var role in loginResponse.UserRoles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return LocalRedirect(returnUrl);

                //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                //if (result.Succeeded)
                //{
                //    _logger.LogInformation("User logged in.");
                //    return LocalRedirect(returnUrl);
                //}
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    _logger.LogWarning("User account locked out.");
                //    return RedirectToPage("./Lockout");
                //}
                //else
                //{
                //    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                //    return Page();
                //}
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
