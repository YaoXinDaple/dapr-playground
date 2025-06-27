using sample.microservice.order;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

const string SELF_APP_ID = "order-service";

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
    //dapr run --app-id "mywebapi" --app-port "6000" --dapr-http-port "6010" -- dotnet run --project ./dapr-webapi.csproj --urls="http://+:6000"
    // ����� Process.Start ��Ҫȷ�� daprd.exe ��·����ȷ
    Process.Start("C:\\dapr\\dapr.exe", $"run --app-id {SELF_APP_ID} --app-port \"5001\" --dapr-http-port \"5010\" --dapr-grpc-port \"50010\" --components-path \".././components\" ");
}

builder.Services.AddControllers().AddDapr(opt =>
{
    opt.UseJsonSerializationOptions(jsonOpt);
    opt.UseHttpEndpoint("http://localhost:5010");
    opt.UseGrpcEndpoint("http://localhost:50010");
});
#else
// ��� Dapr �ͻ���֧�֣������Ҫ����������������״̬��

// Add services to the container.
builder.Services.AddControllers().AddDapr(opt => opt.UseJsonSerializationOptions(jsonOpt));
#endif


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapSubscribeHandler();

app.Run();