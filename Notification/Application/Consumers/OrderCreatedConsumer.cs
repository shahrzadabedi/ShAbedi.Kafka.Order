using MassTransit;
using Notification.Application.DTOs;

namespace Notification.Application.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        public OrderCreatedConsumer()
        {

        }
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            Console.WriteLine($"Notification: Order {context.Message.OrderId} created for customer {context.Message.CustomerName}");

            await Task.Delay(1000);
        }
    }
}

