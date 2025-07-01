// See https://aka.ms/new-console-template for more information
using Dapr.Client;
using Invoice.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("Hello, World!");

var jsonOpt = new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

for (int i = 0; i < 10; i++)
{
    Guid invoiceId = Guid.CreateVersion7();
    InvoiceData invoice = new InvoiceData(invoiceId);
    await Task.Delay(1000);
    using var client = new DaprClientBuilder()
        .UseJsonSerializationOptions(jsonOpt)
        .Build();
    await client.PublishEventAsync("invoiceapi-pubsub", "invoices",invoice);
    Console.WriteLine("Published data: " + invoiceId);
}
