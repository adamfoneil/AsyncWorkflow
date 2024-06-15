using AsyncWorkflow.Records;
using AsyncWorkflow;
using AsyncWorkflow.Interfaces;
using SampleAPI.Models;

namespace SampleAPI.Workers;

public class Step1C(IQueue queue, IStatusRepository<string> statusRepository, ILogger<WorkflowBackgroundService<Document, string>> logger) : WorkflowBackgroundService<Document, string>(queue, statusRepository, logger)
{
	protected override string HandlerName => nameof(Step1C);

	protected override async Task<string> ProcessMessageAsync(Message message, Document payload, CancellationToken stoppingToken)
	{
		var duration = Random.Shared.Next(4, 8) * 1000;
		await Task.Delay(duration, stoppingToken);
		return "Complete";
	}
}
