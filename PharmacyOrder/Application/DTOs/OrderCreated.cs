namespace OrderJobs.Application.DTOs
{
    public class OrderCreated
    {
        public Guid OrderId { get; set; }
        public  string CustomerName { get; set; }
        public  List<OrderItemRequest> Items { get; set; }
    }


    public class OrderItemRequest
    {
        public long ProductId { get; set; }
        public  int Quantity { get; set; }
    }
}
