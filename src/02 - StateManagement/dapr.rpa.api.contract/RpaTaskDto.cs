using System;

namespace dapr.rpa.api.contract
{
    public class RpaTaskDto
    {
        public Guid TaskId { get; set; }
        public decimal Amount { get; set; }
        public string BuyerUscc { get; set; }
        public string SellerUscc { get; set; }
    }
}
