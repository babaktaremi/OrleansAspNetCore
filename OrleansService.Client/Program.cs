using Grains.Interfaces;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();


app.MapGet("/InvokeOrleansService", async (IClusterClient orleansClient , Guid grainId) =>
    {
       var client= orleansClient.GetGrain<IMessagingService>(grainId);

       var response = await client.InvokeMessage($"Message Invoked At {DateTime.Now}");

       return Results.Ok(response);
    })
.WithName("InvokeOrleansService")
.WithOpenApi();


app.MapGet("/InvokeOrleansServiceAutoGrain", async (IClusterClient orleansClient) =>
    {
        var client = orleansClient.GetGrain<IMessagingService>(Guid.NewGuid());

        var response = await client.InvokeMessage($"Message Invoked At {DateTime.Now}");

        return Results.Ok(response);
    })
    .WithName("InvokeOrleansServiceAutoGrain")
    .WithOpenApi();

app.MapGet("/GrainInfo", async (IClusterClient orleansClient, Guid grainId) =>
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

app.Run();

