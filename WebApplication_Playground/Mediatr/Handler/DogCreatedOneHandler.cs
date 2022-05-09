using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WebApplication_Playground.Mediatr.Notification;

namespace WebApplication_Playground.Mediatr.Handler
{
    public sealed class DogCreatedOneHandler : INotificationHandler<DogCreatedNotification>
    {
        public Task Handle(DogCreatedNotification notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{nameof(DogCreatedOneHandler)}: {notification.dog.name} is a very good boy!!");
            return Task.CompletedTask;
        }
    }
}
