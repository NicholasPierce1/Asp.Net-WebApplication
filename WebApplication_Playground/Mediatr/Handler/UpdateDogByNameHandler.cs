using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication_Playground.Mediatr.Command;
using WebApplication_Playground.Repository.Repos;

namespace WebApplication_Playground.Mediatr.Handler
{
    public sealed class UpdateDogByNameHandler : IRequestHandler<UpdateDogByNameCommand, Unit>
    {

        private readonly DogRepository _dogRepository;

        public UpdateDogByNameHandler([FromServices] DogRepository dogRepository)
        {
            this._dogRepository = dogRepository;
        }

        public Task<Unit> Handle(UpdateDogByNameCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Unit>(() => { this._dogRepository.updateDogByName(request.name); return new Unit(); });
        }
    }
}
