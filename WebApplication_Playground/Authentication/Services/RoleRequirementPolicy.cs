using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Authentication.Services
{
    public sealed class RoleRequirementPolicy : IAuthorizationRequirement
    {

        internal readonly string _roleRequired;

        public RoleRequirementPolicy(in string role)
        {
            this._roleRequired = role;
        }

    }
}
