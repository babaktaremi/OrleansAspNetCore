using System.Diagnostics;
using System.Net;
using Grains;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddGrains(builder.Configuration);

builder.Host.UseOrleans(async (context, siloBuilder) =>
{


    siloBuilder
        .UseAdoNetClustering(options =>
        {
            options.Invariant = "System.Data.SqlClient";
            options.ConnectionString =
                context.Configuration.GetConnectionString("OrleansDb");
        })
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "orleansTestCluster";
            options.ServiceId = "orleansTest";
        });

    var activeSilos = GetActiveSiloCount(context.Configuration.GetConnectionString("OrleansDb")!);

    siloBuilder.ConfigureEndpoints(IPAddress.Loopback, 11111 + activeSilos, 30000 + activeSilos);


    siloBuilder.UseDashboard(options =>
    {
        options.Username = "test";
        options.Password = "1234";
        options.Host = "*";
        options.Port = 7070 + activeSilos;
        options.HostSelf = true;
        options.CounterUpdateIntervalMs = 1000;
    });

});

if (!Debugger.IsAttached)
    builder.WebHost.UseUrls("http://127.0.0.1:0");

var app = builder.Build();


app.UseHttpsRedirection();


app.MapGet("/", () => "Orleans Server Is Running!");

await ConfigureMigrationsAsync();

 async Task ConfigureMigrationsAsync()
{
    using var serviceScope = app.Services.CreateScope();

    var messagingDb = serviceScope.ServiceProvider.GetRequiredService<MessagingDbContext>();

    await messagingDb.Database.MigrateAsync();
}

app.Run();

static int GetActiveSiloCount(string connectionString)
{
    const string query = @"SELECT COUNT(*) FROM OrleansMembershipTable WHERE Status = 3"; // 3 is 'Active' status

    using SqlConnection connection = new SqlConnection(connectionString);
    connection.Open();
    using SqlCommand cmd = new SqlCommand(query, connection);
    var result = cmd.ExecuteScalar();
    return Convert.ToInt32(result);
}
