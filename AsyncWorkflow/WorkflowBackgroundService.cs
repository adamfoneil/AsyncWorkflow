using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AsyncWorkflow;

public abstract class WorkflowBackgroundService<TPayload, TKey>(
	IQueue queue,
	IStatusRepository<TKey> statusRepository,
	ILogger<WorkflowBackgroundService<TPayload, TKey>> logger) : BackgroundService
	where TKey : notnull
{
	protected readonly IQueue Queue = queue;
	protected readonly IStatusRepository<TKey> Status = statusRepository;
	protected ILogger<WorkflowBackgroundService<TPayload, TKey>> Logger { get; } = logger;
	protected string MachineName { get; } = Environment.MachineName;

	protected abstract string HandlerName { get; }

	protected abstract Task<string> ProcessMessageAsync(Message message, TPayload payload, CancellationToken stoppingToken);

	protected virtual Task OnCompletedAsync(string status, TPayload payload, CancellationToken stoppingToken) => Task.CompletedTask;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var sw = Stopwatch.StartNew();
				var (status, payload) = await ProcessNextMessageAsync(stoppingToken);
				sw.Stop();

				if (status is not null)
				{
					if (payload is ITrackedPayload<TKey> trackedPayload)
					{
						try
						{
							await Status.SetAsync(new StatusEntry<TKey>(trackedPayload.Key, HandlerName, status, sw.ElapsedMilliseconds));
						}
						catch (Exception exc)
						{
							Logger.LogError(exc, "StatusRepository.SetAsync failed");
						}
					}

					try
					{
						await OnCompletedAsync(status, payload, stoppingToken);
					}
					catch (Exception exc)
					{
						Logger.LogError(exc, "OnCompleted failed");
					}
				}
			}
			catch (Exception exc)
			{
				Logger.LogError(exc, "ExecuteAsync failed");
			}
		}
	}

	public async Task<(string? Status, TPayload Payload)> ProcessNextMessageAsync(CancellationToken stoppingToken)
	{
		try
		{
			var message = await Queue.DequeueAsync(MachineName, HandlerName, stoppingToken);

			if (message is not null)
			{
				try
				{
					Logger.LogDebug("Starting {HandlerName} of message {@message}", HandlerName, message);
					var payload = JsonSerializer.Deserialize<TPayload>(message.Payload) ?? throw new Exception($"Couldn't deserialize payload from queue message Id {message.Id}");
					return (await ProcessMessageAsync(message, payload, stoppingToken), payload);
				}
				catch (Exception exc)
				{
					Logger.LogError(exc, "ProcessNextMessageAsync failed after dequeue {@message}", message);
					await Queue.LogFailureAsync(MachineName, message, exc, stoppingToken);
				}
				finally
				{
					Logger.LogDebug("Finished {HandlerName} of message {@message}", HandlerName, message);
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
