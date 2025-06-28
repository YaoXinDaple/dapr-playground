using System;

namespace dapr.invoiceApi.contract.InvoiceDto
{
    public class InvoiceTask
    {
        public string Buyer { get; set; }
        public string BuyerUscc { get; set; }
        public string Seller { get; set; }
        public string SellerUscc { get; set; }
        public decimal Amount { get; set; }
    }
}
