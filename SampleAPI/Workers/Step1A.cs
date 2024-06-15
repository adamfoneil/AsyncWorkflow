using AsyncWorkflow;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using System.Reflection.Metadata;

namespace SampleAPI.Workers;

public class Step1A(IQueue queue, IStatusRepository<string> statusRepository, ILogger<WorkflowBackgroundService<Document, string>> logger) : WorkflowBackgroundService<Document, string>(queue, statusRepository, logger)
{
	protected override string HandlerName => nameof(Step1A);

	protected override async Task<string> ProcessMessageAsync(Message message, Document payload, CancellationToken stoppingToken)
	{
		var duration = Random.Shared.Next(2, 5) * 1000;
		await Task.Delay(duration, stoppingToken);
		return "Complete";
	}
}
