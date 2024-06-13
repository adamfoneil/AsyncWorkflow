using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AsyncWorkflow;

public abstract class QueueService<TPayload>(string handler, IQueue queue, ILogger<QueueService<TPayload>> logger) : BackgroundService
{
	protected readonly IQueue Queue = queue;

	protected ILogger<QueueService<TPayload>> Logger { get; } = logger;

	protected string MachineName { get; } = Environment.MachineName;

	protected string HandlerName { get; } = handler;

	protected abstract Task ProcessMessageAsync(Message message, TPayload payload, CancellationToken stoppingToken);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{			
			await ProcessNextMessageAsync(stoppingToken);						
		}
	}

	public async Task ProcessNextMessageAsync(CancellationToken stoppingToken)
	{
		var message = await Queue.DequeueAsync(MachineName, HandlerName, stoppingToken);

		if (message is not null)
		{
			var payload = JsonSerializer.Deserialize<TPayload>(message.Payload) ?? throw new Exception($"Couldn't deserialize payload from queue message Id {message.Id}");
			try
			{
				await ProcessMessageAsync(message, payload, stoppingToken);
			}
			catch (Exception exc)
			{
				Logger.LogError(exc, "ProcessMessageFailed, messageId {Id}", message.Id);
				await Queue.LogFailureAsync(MachineName, message, exc, stoppingToken);
			}
		}	
	}
}
