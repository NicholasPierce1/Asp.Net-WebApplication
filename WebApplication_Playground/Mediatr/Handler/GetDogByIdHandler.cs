using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication_Playground.Mediatr.Query;
using WebApplication_Playground.Repository.Entities;
using WebApplication_Playground.Repository.Repos;

namespace WebApplication_Playground.Mediatr.Handler
{
    public sealed class GetDogByIdHandler : IRequestHandler<GetDogByIdQuery, Dog>
    {

        private readonly DogRepository _dogRepository;

        public GetDogByIdHandler([FromServices] DogRepository dogRepository)
        {
            this._dogRepository = dogRepository;
        }
        public Task<Dog> Handle(GetDogByIdQuery request, CancellationToken cancellationToken)
        {
            return Task.Run<Dog>(() => this._dogRepository.getDogById(request.guid));
        }
    }
}
