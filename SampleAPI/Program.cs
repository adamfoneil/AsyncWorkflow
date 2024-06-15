using AsyncWorkflow.DapperSqlServer;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Extensions;
using Microsoft.AspNetCore.Mvc;
using SampleAPI.Models;
using SampleAPI.Workers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultlConnection") ?? throw new Exception("Connection string not found");

builder.Services.Configure<AsyncWorkflowOptions>(builder.Configuration.GetSection("AsyncWorkflow"));
builder.Services.AddDapperSqlServerAsyncWorkflow(connectionString);

builder.Services.AddHostedService<Step1A>();
builder.Services.AddHostedService<Step1B>();
builder.Services.AddHostedService<Step1C>();
builder.Services.AddHostedService<Step2>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/process", async (IQueue queue, [FromBody]Document document) =>
{
	await queue.EnqueueAsync(Environment.MachineName, nameof(Step1A), document);
	await queue.EnqueueAsync(Environment.MachineName, nameof(Step1B), document);
	await queue.EnqueueAsync(Environment.MachineName, nameof(Step1C), document);
	return Results.Ok("Processing started");
});

app.Run();
