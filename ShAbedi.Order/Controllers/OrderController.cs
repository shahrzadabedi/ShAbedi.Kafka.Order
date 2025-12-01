using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShAbedi.Order.Application.Orders;
using ShAbedi.Order.Domain;
using ShAbedi.Order.Persistence;

namespace ShAbedi.Order.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly AppDbContext context;
        
        public OrderController(ILogger<OrderController> logger,
            AppDbContext dbContext)
        {
            _logger = logger;
            context = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            var orderItems = request.Items
                .Select(p => OrderItem.Create(p.ProductId, p.Quantity))
                .ToList();

            var order = Domain.Order.Create(request.CustomerName, orderItems);

            var payload = JsonConvert.SerializeObject(new
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                Items = order.OrderItems.Select(x => new { x.ProductId, x.Quantity })
            });

            var outboxMessage = Outbox.Create("OrderCreated", payload);

            context.Orders.Add(order);
            context.Outbox.Add(outboxMessage);

            await context.SaveChangesAsync(cancellationToken);

            return Ok(new { OrderId = order.Id });
        }
    }
}
