using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Repository.Entities;

namespace WebApplication_Playground.Mediatr.Query
{
    /*
     * All Query classes are for creating read-requests, which consumers (controllers) will "broadcast" to Mediatr
     */
    public sealed class GetAllDogsQuery : IRequest<IEnumerable<Dog>> // IRequest if void-type (no return)
    {

    }
}
