using dapr.rpa.api.contract;
using dapr.rpa.api.Domain;
using Dapr.Client;
using Invoice.Core;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

const string SELF_APP_ID = "rpaclient";
const string USER_APP_ID = "invoiceapi";
const string USER_METHOD_NAME_ACCEPT = "accept";
const string USER_METHOD_NAME_COMPLETE = "complete";

var jsonOpt = new JsonSerializerOptions()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
};
#if DEBUG
//本地调试时，自动启动 Dapr 进程
Process[] processes = Process.GetProcessesByName("daprd");
if (!processes.Any(p => p.GetCommandLineArgs().Contains($" {SELF_APP_ID} ")))
{
    // 获取应用程序基目录，确保 components 路径正确
    var baseDirectory = AppContext.BaseDirectory;
    var componentsPath = Path.Combine(baseDirectory, "components");
    Process.Start(new ProcessStartInfo
    {
        FileName = "C:\\dapr\\dapr.exe",
        Arguments = $"run --app-id {SELF_APP_ID} --app-port \"7001\" --dapr-http-port \"7010\" --dapr-grpc-port \"50002\" --resources-path \"{componentsPath}\"",
        WorkingDirectory = baseDirectory
    });

    //dapr run --app-id "mywebapi" --app-port "6000" --dapr-http-port "6010" -- dotnet run --project ./dapr-webapi.csproj --urls="http://+:6000"
    // 这里的 Process.Start 需要确保 daprd.exe 的路径正确
    //Process.Start("C:\\dapr\\dapr.exe", $"run --app-id {SELF_APP_ID} --app-port \"6001\" --dapr-http-port \"6010\" --resources-path ./components ");
}

builder.Services.AddDaprClient(opt =>
{
    opt.UseJsonSerializationOptions(jsonOpt);
    opt.UseHttpEndpoint("http://localhost:7010");
    opt.UseGrpcEndpoint("http://localhost:50002");
});
#else
// 添加 Dapr 客户端支持（如果需要调用其他服务或操作状态）
builder.Services.AddDaprClient(opt =>
    {
        opt.UseJsonSerializationOptions(jsonOpt);
    });
#endif

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapPost("/accept", async ([FromBody] RpaTaskDto taskDataRequest, [FromServices] DaprClient daprClient) =>
{
    RpaTask rpaTask = new RpaTask
    {
        Id = Guid.CreateVersion7(),
        TaskId = taskDataRequest.TaskId,
        BuyerUscc = taskDataRequest.BuyerUscc,
        SellerUscc = taskDataRequest.SellerUscc,
        Amount = taskDataRequest.Amount
    };
    await daprClient.SaveStateAsync(SELF_APP_ID, rpaTask.TaskId.ToString(), rpaTask);
    return Results.Ok();
});

app.MapPut("/start/{id}", async (Guid id, [FromServices] DaprClient daprClient) =>
{
    RpaTask rpaTask = await daprClient.GetStateAsync<RpaTask>(SELF_APP_ID, id.ToString());
    rpaTask.AcceptedAt = DateTimeOffset.UtcNow;

    await daprClient.SaveStateAsync(SELF_APP_ID, rpaTask.TaskId.ToString(), rpaTask);

    var request = daprClient.CreateInvokeMethodRequest(
        HttpMethod.Put,
        appId: USER_APP_ID,
        methodName: $"{USER_METHOD_NAME_ACCEPT}/{rpaTask.TaskId}",
        null);

    var response = await daprClient.InvokeMethodWithResponseAsync(request);
    response.EnsureSuccessStatusCode();
    return Results.Ok();
});

app.MapPut("/complete/{id}", async (Guid id, [FromServices] DaprClient daprClient) =>
{
    RpaTask rpaTask = await daprClient.GetStateAsync<RpaTask>(SELF_APP_ID, id.ToString());
    rpaTask.CompletedAt = DateTimeOffset.UtcNow;

    await daprClient.SaveStateAsync(SELF_APP_ID, rpaTask.TaskId.ToString(), rpaTask);

    var request = daprClient.CreateInvokeMethodRequest(
        HttpMethod.Put,
        appId: USER_APP_ID,
        methodName: $"{USER_METHOD_NAME_COMPLETE}/{rpaTask.TaskId}");

    var response = await daprClient.InvokeMethodWithResponseAsync(request);
    response.EnsureSuccessStatusCode();
    return Results.Ok();

});

app.Run();
