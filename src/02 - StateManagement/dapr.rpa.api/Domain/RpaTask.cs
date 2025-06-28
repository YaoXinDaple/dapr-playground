namespace dapr.rpa.api.Domain
{
    public class RpaTask
    {
        public Guid TaskId { get; set; }
        public decimal Amount { get; set; }
        public string BuyerUscc { get; set; }
        public string SellerUscc { get; set; }
        public DateTimeOffset? AcceptedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }
        public Guid Id { get; set; }
    }
}
