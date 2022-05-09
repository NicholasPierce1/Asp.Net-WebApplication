using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Mediatr.Command
{
    public sealed class UpdateDogByNameCommand : IRequest<Unit>
    {

        internal readonly string name;

        public UpdateDogByNameCommand ([NotNull] in string name){
            this.name = name;
        }

    }
}
