using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Mediatr.Command;
using WebApplication_Playground.Mediatr.Notification;
using WebApplication_Playground.Mediatr.Query;
using WebApplication_Playground.Repository.Entities;

namespace WebApplication_Playground.Controllers
{
    [AllowAnonymous]
    [Route("api/dog")]
    [ApiController]
    public class DogMediatrController : ControllerBase
    {

        private readonly IMediator _mediatr;

        public DogMediatrController([FromServices] IMediator mediator)
        {
            this._mediatr = mediator;
        }

        [HttpGet]
        [Route("")]
        [Produces("application/json")]
        public async Task<IActionResult> getAllDogs()
        {
            return base.Ok(await this._mediatr.Send<IEnumerable<Dog>>(new GetAllDogsQuery()));
        }

        [HttpGet]
        [Route("getDogById")]
        [Produces("application/json")]
        public async Task<IActionResult> getDogById([FromQuery(Name = "id")] Guid id)
        {
            return base.Ok(await this._mediatr.Send<Dog>(new GetDogByIdQuery(id)));
        }

        [HttpPost]
        [Route("createDog")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IActionResult> createDog([FromBody] Dog dog)
        {
            // return base.Ok(new { guid = await this._mediatr.Send<Guid>(new InsertDogCommand(dog))});

            Guid createdDogGuid = await this._mediatr.Send<Guid>(new InsertDogCommand(dog));

            Dog createdDog = await this._mediatr.Send<Dog>(new GetDogByIdQuery(createdDogGuid));

            await this._mediatr.Publish<DogCreatedNotification>(new DogCreatedNotification(dog));

            return base.Ok(new { guid = createdDogGuid });

        }

        [HttpPut]
        [Route("updateDogByName")]
        [Produces("text/plain")]
        public async Task<IActionResult> updateDogByName([FromQuery(Name = "name")] string name)
        {
            await this._mediatr.Send<Unit>(new UpdateDogByNameCommand(name));
            return base.Ok("ok");
        }


    }
}
