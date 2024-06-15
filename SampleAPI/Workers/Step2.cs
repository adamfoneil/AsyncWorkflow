using AsyncWorkflow.Records;
using AsyncWorkflow;
using AsyncWorkflow.Interfaces;
using SampleAPI.Models;
using AsyncWorkflow.Extensions;

namespace SampleAPI.Workers;

public class Step2(IQueue queue, IStatusRepository<string> statusRepository, ILogger<WorkflowBackgroundService<Document, string>> logger) : WorkflowBackgroundService<Document, string>(queue, statusRepository, logger)
{
	protected override string HandlerName => nameof(Step2);	

	protected override async Task<string> ProcessMessageAsync(Message message, Document payload, CancellationToken stoppingToken)
	{
		var duration = Random.Shared.Next(4, 8) * 1000;
		await Task.Delay(duration, stoppingToken);
		return CompletedStatus;
	}

	public const string CompletedStatus = "Complete";

	public static async Task StartWhenReady(IQueue queue, IStatusRepository<string> status, Document document)
	{
		if (await status.All(document.Key, CompletedStatus, nameof(Step1A), nameof(Step1B), nameof(Step1C)))
		{
			await queue.EnqueuePayloadAsync(Environment.MachineName, nameof(Step2), document);
		}
	}
}
