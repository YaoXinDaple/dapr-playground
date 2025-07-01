using System;
using System.Text.Json.Serialization;

namespace Invoice.Core;

public record InvoiceData
{
    public InvoiceData() { }
    public InvoiceData(Guid invoiceId)
    {
        InvoiceId = invoiceId;
    }
    public Guid InvoiceId { get; set; }
}
