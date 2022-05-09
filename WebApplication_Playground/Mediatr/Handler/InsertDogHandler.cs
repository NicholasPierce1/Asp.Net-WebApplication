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
    public sealed class InsertDogHandler : IRequestHandler<InsertDogCommand, Guid>
    {

        private readonly DogRepository _dogRepository;

        public InsertDogHandler([FromServices] DogRepository dogRepository)
        {
            this._dogRepository = dogRepository;
        }

        public Task<Guid> Handle(InsertDogCommand request, CancellationToken cancellationToken)
        {
            return Task.Run<Guid>(() => this._dogRepository.createDog(request.dog));
        }
    }
}
