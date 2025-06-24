using Dapr.Client;
using dapr_webapi;
using Scalar.AspNetCore;
using System.Diagnostics;
using System.Text.Json;

const string SELF_APP_ID = "mywebapi";

const string HELLO_WORLD_API_ID = "hello-world";
const string HELLO_WORLD_METHOD_HELLO = "hello";

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
    //dapr run --app-id "mywebapi" --app-port "6000" --dapr-http-port "6010" -- dotnet run --project ./dapr-webapi.csproj --urls="http://+:6000"
    // 这里的 Process.Start 需要确保 daprd.exe 的路径正确
    Process.Start("C:\\dapr\\dapr.exe", $"run --app-id {SELF_APP_ID} --app-port \"6001\" --dapr-http-port \"6010\" ");
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

app.MapGet("/hello", async (string value, DaprClient daprClient) =>
{

    #region 第一种方式 CreateHttpClient
    /*
    var client = DaprClient.CreateInvokeHttpClient(appId: HELLO_WORLD_APP_ID);
    var response = await client.GetAsync($"/{HELLO_WORLD_METHOD_HELLO}?value={value}");
    return $"Hello from Dapr + Minimal API! : {await response.Content.ReadAsStringAsync()}";
    */
    #endregion

    #region 第二种方式 使用注入的 DaprClient CreateInvokeMethod
    var request = daprClient.CreateInvokeMethodRequest(
        HttpMethod.Get,
        appId: HELLO_WORLD_API_ID,
        methodName: HELLO_WORLD_METHOD_HELLO,
        new Dictionary<string, string>
        {
            { "value",value} // key=value 格式
        });

    var response = await daprClient.InvokeMethodWithResponseAsync(request);
    return $"Hello from Dapr + Minimal API! : {await response.Content.ReadAsStringAsync()}";
    #endregion
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
