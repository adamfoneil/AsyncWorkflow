namespace AsyncWorkflow;

public interface IQueueStore
{
	Task<string> EnqueueAsync(Message message, CancellationToken cancellationToken);
	Task<Message?> DequeueAsync(string machineName, CancellationToken cancellationToken);
}
