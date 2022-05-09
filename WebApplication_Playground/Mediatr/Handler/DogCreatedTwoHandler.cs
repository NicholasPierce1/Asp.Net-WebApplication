using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApplication_Playground.Mediatr.Notification;

namespace WebApplication_Playground.Mediatr.Handler
{
    public sealed class DogCreatedTwoHandler : INotificationHandler<DogCreatedNotification>
    {
        public Task Handle(DogCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{nameof(DogCreatedTwoHandler)}: {notification.dog.name} is a very good boy indeed!!");
            return Task.CompletedTask;
        }
    }
}
