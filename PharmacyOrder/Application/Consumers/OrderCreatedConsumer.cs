using MassTransit;
using OrderJobs.Application.DTOs;
using PharmacyOrder.Application;
using PharmacyOrder.Domain;
using System.Data;

namespace PharmacyOrder.Application.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PharmacyOrder.Infrastructure.Persistence.AppDbContext _dbContext;

        public OrderCreatedConsumer(IUnitOfWork unitOfWork, PharmacyOrder.Infrastructure.Persistence.AppDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, context.CancellationToken);

                var orderItems = context.Message.Items
                    .Select(item => OrderItem.Create(item.ProductId, item.Quantity))
                    .ToList();

                var order = Order.Create(
                    context.Message.CustomerName,
                    orderItems,
                    OrderStatus.Draft
                );

                await _dbContext.Orders.AddAsync(order, context.CancellationToken);
                await _unitOfWork.CommitAsync(context.CancellationToken);

                Console.WriteLine($"PharmacyOrder: Order {order.Id} created for customer {order.CustomerName} with status {order.Status}");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(context.CancellationToken);
                Console.WriteLine($"Error processing order: {ex.Message}");
                throw;
            }
        }
    }
}
