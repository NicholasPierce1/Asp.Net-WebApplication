using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Repository.Entities;

namespace WebApplication_Playground.Mediatr.Command
{

    public sealed class InsertDogCommand : IRequest<Guid>
    {

        internal readonly Dog dog;

        public InsertDogCommand([NotNull] in Dog dog)
        {
            this.dog = dog;
        }

    }
}
