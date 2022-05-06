using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication_Playground.Authentication.Services
{
    public sealed class RolePolicyAuthorizationHandler : AuthorizationHandler<RoleRequirementPolicy>
    {

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext authorizationHandlerContext,
            RoleRequirementPolicy roleRequirementPolicy)
        {

            ClaimsPrincipal user = authorizationHandlerContext.User;

            Console.Write($"{nameof(RolePolicyAuthorizationHandler)} --> User roles: ");
            foreach (Claim claim in user.Claims.Where<Claim>(claim => claim.Type.Equals(ClaimTypes.Role)))
                Console.Write($"{claim.Value},");
            Console.WriteLine();

            if (user.IsInRole(roleRequirementPolicy._roleRequired))
                authorizationHandlerContext.Succeed(roleRequirementPolicy);
            else
                authorizationHandlerContext.Fail();

            return Task.CompletedTask;

        }

    }
}
