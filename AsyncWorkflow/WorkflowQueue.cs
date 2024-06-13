using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AsyncWorkflow;

public abstract class WorkflowQueue<TPayload, TKey>(
	IQueue queue, 
	IStatusRepository<TKey> statusRepository, 
	ILogger<WorkflowQueue<TPayload, TKey>> logger) : BackgroundService 	
	where TKey : struct	
{
	protected readonly IQueue Queue = queue;
	protected readonly IStatusRepository<TKey> StatusRepository = statusRepository;
	protected ILogger<WorkflowQueue<TPayload, TKey>> Logger { get; } = logger;
	protected string MachineName { get; } = Environment.MachineName;

	protected abstract string HandlerName { get; }

	protected abstract Task<string> ProcessMessageAsync(Message message, TPayload payload, CancellationToken stoppingToken);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var (status, payload) = await ProcessNextMessageAsync(stoppingToken);

				if (status is not null && payload is ITrackedPayload<TKey> trackedPayload)
				{
					try
					{
						var history = new StatusLogEntry<TKey>(trackedPayload.Key, HandlerName, status);
						await StatusRepository.AppendHistoryAsync(history);
					}
					catch (Exception exc)
					{
						Logger.LogError(exc, "StatusRepository.AppendHistory failed");
					}
				}
			}
			catch (Exception exc)
			{
				Logger.LogError(exc, "ExecuteAsync failed");
			}
		}
	}

	public async Task<(string?, TPayload)> ProcessNextMessageAsync(CancellationToken stoppingToken)
	{
		try
		{
			var message = await Queue.DequeueAsync(MachineName, HandlerName, stoppingToken);

			if (message is not null)
			{
				try
				{
					var payload = JsonSerializer.Deserialize<TPayload>(message.Payload) ?? throw new Exception($"Couldn't deserialize payload from queue message Id {message.Id}");

					return (await ProcessMessageAsync(message, payload, stoppingToken), payload);
				}
				catch (Exception exc)
				{
					Logger.LogError(exc, "ProcessNextMessageAsync failed after dequeue {@message}", message);
					await Queue.LogFailureAsync(MachineName, message, exc, stoppingToken);
				}
			}
		}
		catch (Exception exc)
		{
			Logger.LogError(exc, "ProcessNextMessageAsync failed on dequeue");
		}		

		return default;
	}
}
