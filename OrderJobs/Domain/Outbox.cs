namespace ShAbedi.Order.Domain
{
    public class Outbox
    {
        public Guid Id { get; private set; }
        public string Type { get; private set; }          // مثلا "OrderCreated"
        public string Payload { get; private set; }       // JSON
        public DateTime OccurredOn { get; private set; }
        public DateTime? ProcessedOn { get; private set; }
        public bool IsProcessed { get; private set; }
        public int RetryCount { get; private set; }

        private Outbox() { }

        public static Outbox Create(string type, string payload)
        {
            return new Outbox()
            {
                Id = Guid.NewGuid(),
                Type = type,
                Payload = payload,
                OccurredOn = DateTime.UtcNow,
                IsProcessed = false,
                RetryCount = 0
            };
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
            ProcessedOn = DateTime.UtcNow;
        }

        public void IncreaseRetry()
        {
            RetryCount++;
        }
    }
}
