﻿using AsyncWorkflow;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using SampleAPI.Models;

namespace SampleAPI.Workers;

public class Step1B(IQueue queue, IStatusRepository<string> statusRepository, ILogger<WorkflowBackgroundService<Document, string>> logger) : WorkflowBackgroundService<Document, string>(queue, statusRepository, logger)
{
	protected override string HandlerName => nameof(Step1B);

	protected override async Task<string> ProcessMessageAsync(Message message, Document payload, CancellationToken stoppingToken)
	{
		var duration = Random.Shared.Next(3, 7) * 1000;
		await Task.Delay(duration, stoppingToken);
		return Step2.CompletedStatus;
	}

	protected override async Task OnCompletedAsync(string status, Document payload, CancellationToken stoppingToken) =>
		await Step2.StartWhenReady(Queue, Status, payload);
}
