using Rpa.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<RpaActor>();
    options.ReentrancyConfig = new Dapr.Actors.ActorReentrancyConfig()
    {
        Enabled = true,
        MaxStackDepth = 32,
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapActorsHandlers();

app.Run();


/*
 接收Invoice 开票信息，根据Id创建一个 RpaActor 实例，
RpaActor 实例将处理开票任务，提供一个接口，可以让RpaActor返回实时处理状态
RpaActor 处理完成之后，通知Invoice.Api开票结果
需要 ControllerActor控制RpaActor相关的状态？
 */
