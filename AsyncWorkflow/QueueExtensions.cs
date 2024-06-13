using System.Text.Json;

namespace AsyncWorkflow;

public static class QueueExtensions
{
	public static async Task<string> EnqueueAsync<TPayload>(this IQueue queue, string machineName, TPayload payload, string? userName = null)
	{
		var json = JsonSerializer.Serialize(payload);
		var message = new Message(json, typeof(TPayload).FullName, userName);
		return await queue.EnqueueAsync(machineName, message);
	}	
}
