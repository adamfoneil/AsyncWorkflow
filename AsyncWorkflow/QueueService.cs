using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsyncWorkflow;

public abstract class QueueService(IQueueStore queueStore, ILogger<QueueService> logger) : BackgroundService
{
	protected readonly IQueueStore Store = queueStore;

	protected ILogger<QueueService> Logger { get; } = logger;

	protected string MachineName { get; } = Environment.MachineName;

	protected abstract Task ProcessMessageAsync(Message message, CancellationToken stoppingToken);

	protected virtual async Task OnProcessMessageFailedAsync(Message message) => await Task.CompletedTask;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{			
			await ProcessNextMessageAsync(stoppingToken);						
		}
	}

	public async Task ProcessNextMessageAsync(CancellationToken stoppingToken)
	{
		var message = await Store.DequeueAsync(MachineName, stoppingToken);

		if (message is not null)
		{
			try
			{
				await ProcessMessageAsync(message, stoppingToken);
			}
			catch (Exception exc)
			{
				Logger.LogError(exc, "Error in DoWorkAsync, messageId {Id}", message.Id);
				await OnProcessMessageFailedAsync(message);
			}
		}	
	}
}
