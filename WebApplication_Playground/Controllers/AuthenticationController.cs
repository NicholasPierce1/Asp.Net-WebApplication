using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using WebApplication_Playground.Models.RestModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using WebApplication_Playground.Models.Configuration;
using Microsoft.Extensions.Options;
using WebApplication_Playground.DepedencyInjection;
using Microsoft.AspNetCore.Authorization;
using WebApplication_Playground.Authentication.Model;
using WebApplication_Playground.Authentication.Services;
using System.Text.RegularExpressions;

namespace WebApplication_Playground.Controllers
{
    [Route("api/authenticate")]
    [ApiController]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {

        private IUserService _userService;

        public AuthenticationController(IUserService userService)
        {
            this._userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateModel model)
        {
            User user = await _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [HttpGet]
        [Route("")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<User> users = await _userService.GetAll();
            return Ok(users);
        }

        [HttpGet]
        [Route("secret")]
        [Produces("application/json")]
        [Authorize(Roles = "Admin,Owner")]
        public IActionResult GetSecret()
        {
            return Ok(new {message = "you got the secret" });
        }

        [HttpGet]
        [Route("Employee")]
        [Produces("application/json")]
        [Authorize(Roles = "Employee")]
        public IActionResult GetEmployee()
        {
            return Ok(new { message = "you are an employee" });
        }

        [HttpGet]
        [Route("getUser")]
        [Produces("application/json")]
        public IActionResult GetPrincipalUser()
        {
            ClaimsPrincipal user = base.User;

            List<Dictionary<string, object>> userClaims = new List<Dictionary<string, object>>();

            foreach (Claim claim in user.Claims)
            {

                Regex regex = new Regex("^(?:http://)([^/]+/?)*$");

                Match match = regex.Match(claim.Type);

                userClaims.Add(
                    new Dictionary<string, object>()
                    {
                        {nameof(claim.Type), claim.Type },
                        {"Type_Simplified", match.Groups[1].Captures[match.Groups[1].Captures.Count -1].Value },
                        {nameof(claim.Value), claim.Value },
                        {nameof(claim.Issuer), claim.Issuer },
                        // {nameof(claim.Subject), claim.Subject },
                        {nameof(claim.ValueType), claim.ValueType },
                        {nameof(claim.Properties), claim.Properties }
                    }
                );
            }

            Console.WriteLine($"{nameof(this.GetPrincipalUser)}: size of claims({userClaims.Count})");

            return base.Ok(
                new Dictionary<string, object>() {
                    {"name", user.Identity.Name},
                    {"AuthenticationType", user.Identity.AuthenticationType ?? "N/A" },
                    {"isAuthenticated", user.Identity.IsAuthenticated},
                    {nameof(userClaims), userClaims }
                }
            );
        }

    }
}
