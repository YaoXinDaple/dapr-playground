using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Client;
using Invoice.Core;
using Microsoft.AspNetCore.Mvc;
using Rpa.Client;
using Scalar.AspNetCore;
using System.Diagnostics;
using System.Text.Json;

const string SELF_APP_ID = "invoiceapi";

var rpaWorkerActorType = "RpaActor";
var controllerActorType = "ControllerActor";

var builder = WebApplication.CreateBuilder(args);

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
    // 获取应用程序基目录，确保components路径正确
    var baseDirectory = AppContext.BaseDirectory;
    var componentsPath = Path.Combine(baseDirectory, "components");
    Process.Start(new ProcessStartInfo
    {
        FileName = "C:\\dapr\\dapr.exe",
        Arguments = $"run --app-id {SELF_APP_ID} --app-port \"6001\" --dapr-http-port \"6010\" --dapr-grpc-port \"50001\" --resources-path \"{componentsPath}\"",
        WorkingDirectory = baseDirectory
    });

    //dapr run --app-id "mywebapi" --app-port "6000" --dapr-http-port "6010" -- dotnet run --project ./dapr-webapi.csproj --urls="http://+:6000"
    // 这里的 Process.Start 需要确保 daprd.exe 的路径正确
    //Process.Start("C:\\dapr\\dapr.exe", $"run --app-id {SELF_APP_ID} --app-port \"6001\" --dapr-http-port \"6010\" --resources-path ./components ");
}

builder.Services.AddDaprClient(opt =>
{
    opt.UseJsonSerializationOptions(jsonOpt);
    opt.UseHttpEndpoint("http://localhost:6010");
    opt.UseGrpcEndpoint("http://localhost:50001");
});
#else
// 添加 Dapr 客户端支持（如果需要调用其他服务或操作状态）
builder.Services.AddDaprClient(opt =>
    {
        opt.UseJsonSerializationOptions(jsonOpt);
    });
#endif

builder.Services.AddOpenApi();



var app = builder.Build();

// 使用 Minimal API 风格注册路由（自动包含 UseRouting）
app.MapSubscribeHandler();


app.MapPost("/task", async ([FromServices] DaprClient daprClient) =>
{
    try
    {
        Guid actorId = Guid.CreateVersion7();
        var rpaActor1 = new ActorId(actorId.ToString());

        var proxyOptions = new ActorProxyOptions
        {
            HttpEndpoint = "http://localhost:56001",  // 修改为 rpaclient 的 dapr-http-port
            DaprApiToken = null
        };

        var rpaWorker1 = ActorProxy.Create<IRpaWorker>(
            rpaActor1,
            rpaWorkerActorType,
            proxyOptions);  // 使用配置好的 proxyOptions

        await rpaWorker1.CreateAsync(actorId);
        return Results.Ok(actorId);
    }
    catch (Exception e)
    {

        throw;
    }
});


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
