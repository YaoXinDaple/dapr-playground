using Dapr.Client;
using dapr.minimalApi;
using Microsoft.AspNetCore.Mvc;
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

app.MapGet("/hello", async (string value, DaprClient daprClient) =>
{

    #region ��һ�ַ�ʽ CreateHttpClient
    /*
    var client = DaprClient.CreateInvokeHttpClient(appId: HELLO_WORLD_APP_ID);
    var response = await client.GetAsync($"/{HELLO_WORLD_METHOD_HELLO}?value={value}");
    return $"Hello from Dapr + Minimal API! : {await response.Content.ReadAsStringAsync()}";
    */
    #endregion

    #region �ڶ��ַ�ʽ ʹ��ע��� DaprClient CreateInvokeMethod
    var request = daprClient.CreateInvokeMethodRequest(
        HttpMethod.Get,
        appId: HELLO_WORLD_API_ID,
        methodName: HELLO_WORLD_METHOD_HELLO,
        new Dictionary<string, string>
        {
            { "value",value} // key=value ��ʽ
        });

    var response = await daprClient.InvokeMethodWithResponseAsync(request);
    return $"Hello from Dapr + Minimal API! : {await response.Content.ReadAsStringAsync()}";
    #endregion
});

//����������Dapr StateManagement ʾ��
app.MapGet("/state/{key}", async ([FromRoute]string key, DaprClient daprClient) =>
{
    try
    {
        var state = await daprClient.GetStateAsync<string>("mywebapi", key);
        return state ?? "State not found";
    }
    catch (Exception e)
    {

        throw;
    }
});

app.MapPost("/state/{key}", async ([FromRoute] string key, string value, DaprClient daprClient) =>
{
    await daprClient.SaveStateAsync("mywebapi", key, value);
    return Results.Ok($"State saved: {key} = {value}");
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();
