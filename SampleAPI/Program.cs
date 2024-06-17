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

app.MapGet("/", () => "Use the /process POST endpoint to test a simple workflow");

app.MapPost("/process", async (IQueue queue, Document document) =>
{
	// the first 3 processes run in parallel
	await queue.EnqueuePayloadAsync(nameof(Step1A), document);
	await queue.EnqueuePayloadAsync(nameof(Step1B), document);
	await queue.EnqueuePayloadAsync(nameof(Step1C), document);
	// Step2 runs automatically after the first 3 are finished
	return Results.Ok("Processing started");
});

app.Run();
