namespace ShAbedi.Order.Application.Orders
{
    public class CreateOrderRequest
    {
        public required List<OrderItemRequest> Items { get; set; }
        public required string CustomerName { get; set; } 
    }

    public class OrderItemRequest
    {
        public required long ProductId { get; set; }
        public required int Quantity { get; set; }
    }
}
