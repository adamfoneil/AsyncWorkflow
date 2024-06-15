using AsyncWorkflow.Records;
using AsyncWorkflow;
using AsyncWorkflow.Interfaces;
using SampleAPI.Models;

namespace SampleAPI.Workers;

public class Step2(IQueue queue, IStatusRepository<string> statusRepository, ILogger<WorkflowBackgroundService<Document, string>> logger) : WorkflowBackgroundService<Document, string>(queue, statusRepository, logger)
{
	protected override string HandlerName => nameof(Step1B);

	protected override Task<string> ProcessMessageAsync(Message message, Document payload, CancellationToken stoppingToken)
	{
		throw new NotImplementedException();
	}
}
