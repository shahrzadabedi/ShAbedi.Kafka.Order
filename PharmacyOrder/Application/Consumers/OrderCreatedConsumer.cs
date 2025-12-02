using MassTransit;
using OrderJobs.Application.DTOs;

namespace PharmacyOrder.Application.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        public OrderCreatedConsumer()
        {

        }
        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            Console.WriteLine("Test");

            await Task.Delay(1000);
        }
    }
}
