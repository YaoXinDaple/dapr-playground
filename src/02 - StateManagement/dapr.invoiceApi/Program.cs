using dapr.invoiceApi.contract.InvoiceDto;
using dapr.minimalApi.Domain;
using dapr.rpa.api.contract;
using Dapr.Client;
using Invoice.Core;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Diagnostics;
using System.Text.Json;

const string SELF_APP_ID = "invoiceapi";

const string RPA_API_ID = "rpaclient";
const string RPA_METHOD_ACCEPT = "accept";

var builder = WebApplication.CreateBuilder(args);

var jsonOpt = new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
};
#if DEBUG
//���ص���ʱ���Զ����� Dapr ����
Process[] processes = Process.GetProcessesByName("daprd");
if (!processes.Any(p => p.GetCommandLineArgs().Contains($" {SELF_APP_ID} ")))
{
    // ��ȡӦ�ó����Ŀ¼��ȷ��components·����ȷ
    var baseDirectory = AppContext.BaseDirectory;
    var componentsPath = Path.Combine(baseDirectory, "components");
    Process.Start(new ProcessStartInfo
    {
        FileName = "C:\\dapr\\dapr.exe",
        Arguments = $"run --app-id {SELF_APP_ID} --app-port \"6001\" --dapr-http-port \"6010\" --dapr-grpc-port \"50001\" --resources-path \"{componentsPath}\"",
        WorkingDirectory = baseDirectory
    });

    //dapr run --app-id "mywebapi" --app-port "6000" --dapr-http-port "6010" -- dotnet run --project ./dapr-webapi.csproj --urls="http://+:6000"
    // ����� Process.Start ��Ҫȷ�� daprd.exe ��·����ȷ
    //Process.Start("C:\\dapr\\dapr.exe", $"run --app-id {SELF_APP_ID} --app-port \"6001\" --dapr-http-port \"6010\" --resources-path ./components ");
}

builder.Services.AddDaprClient(opt =>
{
    opt.UseJsonSerializationOptions(jsonOpt);
    opt.UseHttpEndpoint("http://localhost:6010");
    opt.UseGrpcEndpoint("http://localhost:50001");
});
#else
// ��� Dapr �ͻ���֧�֣������Ҫ����������������״̬��
builder.Services.AddDaprClient(opt =>
    {
        opt.UseJsonSerializationOptions(jsonOpt);
    });
#endif

builder.Services.AddOpenApi();



var app = builder.Build();

// ʹ�� Minimal API ���ע��·�ɣ��Զ����� UseRouting��
app.MapSubscribeHandler();


app.MapPost("/task", async ([FromBody] InvoiceTask taskDataRequest, [FromServices] DaprClient daprClient) =>
{
    TaskData task = new TaskData
    {
        Buyer = taskDataRequest.Buyer,
        BuyerUscc = taskDataRequest.BuyerUscc,
        Seller = taskDataRequest.Seller,
        SellerUscc = taskDataRequest.SellerUscc,
        Amount = taskDataRequest.Amount
    };
    await daprClient.SaveStateAsync(SELF_APP_ID, task.Id.ToString(), task);

    RpaTaskDto dto = new RpaTaskDto
    {
        TaskId = task.Id,
        BuyerUscc = task.BuyerUscc,
        SellerUscc = task.SellerUscc,
        Amount = task.Amount
    };

    var request = daprClient.CreateInvokeMethodRequest(
        HttpMethod.Post,
        appId: RPA_API_ID,
        methodName: RPA_METHOD_ACCEPT,
        null,
        dto);

    var response = await daprClient.InvokeMethodWithResponseAsync(request);
    response.EnsureSuccessStatusCode();
    task.StartedAt = DateTimeOffset.UtcNow;

    return Results.Accepted(task.Id.ToString());
});

app.MapPut("/accept/{id}", async (Guid id, [FromServices] DaprClient daprClient) =>
{
    TaskData data = await daprClient.GetStateAsync<TaskData>(SELF_APP_ID, id.ToString());
    data.StartedAt = DateTimeOffset.UtcNow;

    await daprClient.SaveStateAsync(SELF_APP_ID, data.Id.ToString(), data);
    return Results.Ok();
});

app.MapPut("/complete/{id}", async (Guid id, [FromServices] DaprClient daprClient) =>
{
    TaskData data = await daprClient.GetStateAsync<TaskData>(SELF_APP_ID, id.ToString());
    data.CompletedAt = DateTimeOffset.UtcNow;

    await daprClient.SaveStateAsync(SELF_APP_ID, data.Id.ToString(), data);
    return Results.Ok();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
