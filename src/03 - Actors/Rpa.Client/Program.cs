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
 ����Invoice ��Ʊ��Ϣ������Id����һ�� RpaActor ʵ����
RpaActor ʵ��������Ʊ�����ṩһ���ӿڣ�������RpaActor����ʵʱ����״̬
RpaActor �������֮��֪ͨInvoice.Api��Ʊ���
��Ҫ ControllerActor����RpaActor��ص�״̬��
 */
