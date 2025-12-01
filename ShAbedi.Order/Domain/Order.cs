namespace ShAbedi.Order.Domain
{
    public class Order
    {
        public Guid Id { get; private set; }
        public IReadOnlyCollection<OrderItem> OrderItems { get; private set; }
        public string CustomerName { get; private set; }

        public static Order Create(string customerName, List<OrderItem> orderItems)
        {
            var order = new Order()
            {
                Id = Guid.NewGuid(),
                CustomerName = customerName,
                OrderItems = orderItems
            };

            return order;
        }
    }

    public class OrderItem
    {
        public Guid Id { get; private set; }
        public long ProductId { get; private set; }
        public int Quantity { get; private set; }

        public static OrderItem Create(long productId, int quantity)
        {
            var orderItem = new OrderItem()
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantity
            };

            return orderItem;
        }
    }
}
