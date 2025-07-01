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
    // 添加这两行
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

// 添加 Dapr 客户端支持（如果需要调用其他服务或操作状态）
builder.Services.AddDaprClient(opt =>
{
    opt.UseJsonSerializationOptions(jsonOpt);
});

builder.Services.AddOpenApi();


var app = builder.Build();

// 使用 Minimal API 风格注册路由（自动包含 UseRouting）
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

