using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using AsyncWorkflow;
using SampleAPI.Models;

namespace SampleAPI.Workers;

public class Step1B(IQueue queue, IStatusRepository<string> statusRepository, ILogger<WorkflowBackgroundService<Document, string>> logger) : WorkflowBackgroundService<Document, string>(queue, statusRepository, logger)
{
	protected override string HandlerName => nameof(Step1B);

	protected override async Task<string> ProcessMessageAsync(Message message, Document payload, CancellationToken stoppingToken)
	{
		var duration = Random.Shared.Next(3, 7) * 1000;
		await Task.Delay(duration, stoppingToken);
		return "Complete";
	}
}
