using Dapr;
using Invoice.Core;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

var jsonOpt = new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    // ���������
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

// ��� Dapr �ͻ���֧�֣������Ҫ����������������״̬��
builder.Services.AddDaprClient(opt =>
{
    opt.UseJsonSerializationOptions(jsonOpt);
});

builder.Services.AddOpenApi();


var app = builder.Build();

// ʹ�� Minimal API ���ע��·�ɣ��Զ����� UseRouting��
app.MapSubscribeHandler();

app.MapPost("/complete", [Topic("invoiceapi-pubsub", "invoices")] (CloudEvent<InvoiceData> cloudEvent) =>
{
    var invoice = cloudEvent.Data;
    //Console.WriteLine($"Received cloud event: {JsonSerializer.Serialize(cloudEvent, jsonOpt)}");
    Console.WriteLine($"Invoice Id:{invoice.InvoiceId} has been processed by rpa server");
    return Results.Ok(invoice);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();

