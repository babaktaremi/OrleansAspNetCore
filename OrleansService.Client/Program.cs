using Grains.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Orleans.Configuration;
using OrleansService.Client.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
});

builder.Host.UseOrleansClient(clientBuilder =>
{
    clientBuilder
        .UseAdoNetClustering(options =>
        {
            options.Invariant = "System.Data.SqlClient";
            options.ConnectionString =
                "Data Source=localhost;Initial Catalog=OrleansClusterManagement;Integrated Security=true;Encrypt=False";
        })
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "orleansTestCluster";
            options.ServiceId = "orleansTest";
        });

}).ConfigureLogging(c => c.AddConsole());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();



app.MapGet("/InvokeOrleansService", async (IGrainFactory orleansClient , Guid grainId) =>
    {
       var client= orleansClient.GetGrain<IMessagingService>(grainId);

       var response = await client.InvokeMessage($"Message Invoked At {DateTime.Now}");

       return Results.Ok(response);
    })
.WithName("InvokeOrleansService")
.WithOpenApi();


app.MapGet("/InvokeOrleansServiceAutoGrain", async (IGrainFactory orleansClient
    ,IHubContext<NotificationHub,INotificationHub> hub) =>
    {
        var client = orleansClient.GetGrain<IMessagingService>(Guid.NewGuid());

        var response = await client.InvokeMessage($"Message Invoked At {DateTime.Now}");

        await hub.Clients.All.PublishMessageToHub(response);

        return Results.Ok(response);
    })
    .WithName("InvokeOrleansServiceAutoGrain")
    .WithOpenApi()
    ;

app.MapGet("/GrainInfo", async (IGrainFactory orleansClient, Guid grainId) =>
    {
        var client = orleansClient.GetGrain<IMessagingService>(grainId);

        var response = await client.GetGrainInfo();

        return Results.Ok(new
        {
            response.ActivationDate,
            response.GrainId
        });
    })
    .WithName("GetGrainInfo")
    .WithOpenApi();

app.MapHub<NotificationHub>("/NotificationHub");

app.Run();

