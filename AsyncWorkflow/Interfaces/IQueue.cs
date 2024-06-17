using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IQueue
{
	Task EnqueueAsync(string machineName, Message message);
	Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken);
	Task LogFailureAsync<TPayload, TKey>(string machineName, string handler, TPayload payload, Exception exception, CancellationToken cancellationToken) where TKey : notnull;
}
