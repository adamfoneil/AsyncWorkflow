using AsyncWorkflow.DapperSqlServer;
using AsyncWorkflow.Extensions;
using AsyncWorkflow.Interfaces;
using SampleAPI.Models;
using SampleAPI.Workers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not found");

builder.Services.Configure<AsyncWorkflowOptions>(builder.Configuration.GetSection("AsyncWorkflow"));
builder.Services.AddDapperSqlServerAsyncWorkflow(connectionString);

builder.Services.AddHostedService<Step1A>();
builder.Services.AddHostedService<Step1B>();
builder.Services.AddHostedService<Step1C>();
builder.Services.AddHostedService<Step2>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDapperSqlServerAsyncWorkflow();

app.MapPost("/process", async (IQueue queue, Document document) =>
{
	await queue.EnqueuePayloadAsync(Environment.MachineName, nameof(Step1A), document);
	await queue.EnqueuePayloadAsync(Environment.MachineName, nameof(Step1B), document);
	await queue.EnqueuePayloadAsync(Environment.MachineName, nameof(Step1C), document);
	return Results.Ok("Processing started");
});

app.Run();
