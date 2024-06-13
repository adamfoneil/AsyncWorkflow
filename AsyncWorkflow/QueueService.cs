using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsyncWorkflow;

public abstract class QueueService(IQueue queue, ILogger<QueueService> logger) : BackgroundService
{
	protected readonly IQueue Queue = queue;

	protected ILogger<QueueService> Logger { get; } = logger;

	protected string MachineName { get; } = Environment.MachineName;

	protected abstract Task ProcessMessageAsync(Message message, CancellationToken stoppingToken);

	protected virtual async Task ProcessMessageFailedAsync(Exception exception, Message message)
	{
		Logger.LogError(exception, "ProcessMessageFailed, messageId {Id}", message.Id);
		await Task.CompletedTask;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{			
			await ProcessNextMessageAsync(stoppingToken);						
		}
	}

	public async Task ProcessNextMessageAsync(CancellationToken stoppingToken)
	{
		var message = await Queue.DequeueAsync(MachineName, stoppingToken);

		if (message is not null)
		{
			try
			{
				await ProcessMessageAsync(message, stoppingToken);
			}
			catch (Exception exc)
			{				
				await ProcessMessageFailedAsync(exc, message);
			}
		}	
	}
}
