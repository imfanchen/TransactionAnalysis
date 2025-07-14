using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole();
builder.Logging.AddJsonConsole();
builder.Services.AddHttpClient<TransactionService>().AddStandardResilienceHandler();
builder.Services.AddHostedService<TransactionService>();

using IHost host = builder.Build();
await host.RunAsync();

