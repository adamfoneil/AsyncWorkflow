using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AsyncWorkflow;

public abstract class WorkflowQueue<TPayload, TStatus>(
	IQueue queue, 
	ITrackingRepository<TStatus> trackingRepository, 
	ILogger<WorkflowQueue<TPayload, TStatus>> logger) : BackgroundService 
	where TStatus : Enum	
{
	protected readonly IQueue Queue = queue;
	protected readonly ITrackingRepository<TStatus> TrackingRepository = trackingRepository;
	protected ILogger<WorkflowQueue<TPayload, TStatus>> Logger { get; } = logger;
	protected string MachineName { get; } = Environment.MachineName;

	protected abstract string HandlerName { get; }

	protected abstract Task<TStatus> ProcessMessageAsync(Message message, TPayload payload, CancellationToken stoppingToken);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var (status, payload) = await ProcessNextMessageAsync(stoppingToken);

				if (status is not null && payload is ITrackedPayload trackedPayload)
				{
					var history = new StatusLogEntry<TStatus>(trackedPayload.Key, HandlerName, status);
					await TrackingRepository.AppendHistoryAsync(history);
				}
			}
			catch (Exception exc)
			{
				Logger.LogError(exc, "ProcessNextMessage failed");
			}			
		}
	}

	public async Task<(TStatus?, TPayload)> ProcessNextMessageAsync(CancellationToken stoppingToken)
	{
		var message = await Queue.DequeueAsync(MachineName, HandlerName, stoppingToken);

		if (message is not null)
		{
			var payload = JsonSerializer.Deserialize<TPayload>(message.Payload) ?? throw new Exception($"Couldn't deserialize payload from queue message Id {message.Id}");

			try
			{
				return (await ProcessMessageAsync(message, payload, stoppingToken), payload);
			}
			catch (Exception exc)
			{
				Logger.LogError(exc, "ProcessMessageFailed, messageId {Id}", message.Id);
				await Queue.LogFailureAsync(MachineName, message, exc, stoppingToken);
			}			
		}

		return default;
	}
}
