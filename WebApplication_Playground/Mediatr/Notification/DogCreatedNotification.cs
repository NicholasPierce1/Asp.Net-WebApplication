using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using WebApplication_Playground.Repository.Entities;

namespace WebApplication_Playground.Mediatr.Notification
{
    public sealed class DogCreatedNotification : INotification
    {

        internal Dog dog;

        public DogCreatedNotification([NotNull] in Dog dog)
        {
            this.dog = dog;
        }

    }
}
