using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication_Playground.Mediatr.Query;
using WebApplication_Playground.Repository.Entities;
using WebApplication_Playground.Repository.Repos;

namespace WebApplication_Playground.Mediatr.Handler
{
    // No need to create IoC for handlers and requests : )
    // Mediatr's dependency injection extension handles that for us
    public class GetAllDogsHandler : IRequestHandler<GetAllDogsQuery, IEnumerable<Dog>>
    {

        private readonly DogRepository _dogRepository;

        public GetAllDogsHandler([FromServices] DogRepository dogRepository)
        {
            this._dogRepository = dogRepository;
        }

        // add 'async' if you do awaited operations
        // else, create manual task in hot-state
        public Task<IEnumerable<Dog>> Handle(GetAllDogsQuery request, CancellationToken cancellationToken)
        {
            return Task.Run<IEnumerable<Dog>>(this._dogRepository.getAllDogs);
        }
    }
}
