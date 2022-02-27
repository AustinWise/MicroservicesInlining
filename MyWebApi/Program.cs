using Microsoft.Extensions.DependencyInjection.Extensions;
using MyWebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.TryAddSingleton<InlineClient>();

IServiceProvider? serviceProvider = null;
builder.Services.AddGrpcClient<MyGrpcService.Greeter.GreeterClient>(o =>
{
    o.Creator = _ => serviceProvider!.GetService<InlineClient>()!;
    o.Address = new Uri("https://localhost:7119");
});

var app = builder.Build();
serviceProvider = app.Services;

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
