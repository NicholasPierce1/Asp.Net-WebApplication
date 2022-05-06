using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApplication_Playground.Authentication.Model;
using WebApplication_Playground.Authentication.Services;

namespace WebApplication_Playground.Controllers
{
    [Route("api/bearerAuthentication")]
    [ApiController]
    public class BearerAuthenticationController : ControllerBase
    {

        private readonly IUserService _userService;

        private readonly IConfiguration _configuration;

        public BearerAuthenticationController([FromServices] IUserService userService, [FromServices] IConfiguration configuration)
        {
            this._userService = userService;
            this._configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> postToken([FromBody] User user)
        {
            Console.WriteLine($"{nameof(BearerAuthenticationController)}.{nameof(this.postToken)}: called!");
            user = await this._userService.Authenticate(user.Username, user.Password);

            if (user == null)
                return base.Problem(
                    detail: "User does not exist with username or password",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "login failed"
                );
            
            /*
             * can add so many more claims (email, name id, etc.).
             * can even add custom ones --> just replace claim type with a string
             * can extract from principal using a claim type or just a key
             * claimsPrincipal
             */
            IList<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (string role in user.Roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // subject
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Username));

            // guarantee unique
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            // create symmetric key 
            SigningCredentials signing = new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            this._configuration.GetValue<string>("Jwt:secret")
                        )
                    ),
                    SecurityAlgorithms.HmacSha256
                );

            // creates security token w/ expiration for 20 minutes
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                    issuer: this._configuration.GetValue<string>("Jwt:Issuer"),
                    audience: this._configuration.GetValue<string>("Jwt:Audience"),
                    expires: DateTime.UtcNow.AddMinutes(20),
                    claims: claims,
                    signingCredentials: signing
                );

            return base.Ok(
                    new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                        expiresIn = DateTime.UtcNow.AddMinutes(20).ToLocalTime().ToString("MM/dd/yyyy_hh:mm:ss"),
                        tokenType = "Bearer"
                    }
                );

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("")]
        [Produces("text/plain")]
        public IActionResult getPrincipalUserName()
        {
            return base.Ok($"{base.User.Identity.Name}");
        }

        // policy AND role constraint
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Employee", Policy = "EmployeeRoleRequirementPolicy")]
        [HttpGet]
        [Route("getPrincipalUserNamAndEmployeeRole")]
        [Produces("application/json")]
        public IActionResult getPrincipalUserNamAndEmployeeRolee()
        {
            return base.Ok(
                new
                {
                    name = base.User.Identity.Name,
                    roles = 
                        (from Claim claim in base.User.Claims.Where<Claim>( claim => claim.Type.Equals(ClaimTypes.Role) )
                        select
                            claim.Value)
                }
            );
        }

    }
}
