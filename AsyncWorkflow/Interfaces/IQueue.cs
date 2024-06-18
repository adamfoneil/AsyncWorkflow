using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IQueue
{
	Task EnqueueAsync(string machineName, Message message);
	Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken);
	Task LogErrorAsync<TKey>(string machineName, string handler, ITrackedPayload<TKey> payload, Exception exception, CancellationToken cancellationToken) where TKey : notnull;
}
