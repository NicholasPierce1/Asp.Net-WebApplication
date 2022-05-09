using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using WebApplication_Playground.Repository.Entities;

namespace WebApplication_Playground.Mediatr.Query
{
    public sealed class GetDogByIdQuery : IRequest<Dog>
    {

        internal readonly Guid guid;

        public GetDogByIdQuery([NotNull] in Guid guid)
        {
            this.guid = guid;
        }

    }
}
