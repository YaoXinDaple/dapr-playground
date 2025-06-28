namespace dapr.minimalApi.Domain
{
    public class TaskData
    {
        public string Buyer { get; set; }
        public string BuyerUscc { get; set; }
        public string Seller { get; set; }
        public string SellerUscc { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid Id { get; set; } = Guid.CreateVersion7();

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }
    }
}
