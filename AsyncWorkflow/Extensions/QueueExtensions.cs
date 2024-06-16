using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using System.Text.Json;

namespace AsyncWorkflow.Extensions;

public static class QueueExtensions
{
	public static async Task<(string, DateTime)> EnqueuePayloadAsync<TPayload>(this IQueue queue, string machineName, string handler, TPayload payload) where TPayload : notnull
	{
		var json = JsonSerializer.Serialize(payload);
		var message = new Message(handler, json);
		await queue.EnqueueAsync(machineName, message);
		return (message.Id, message.Timestamp);
	}
}
