namespace AsyncWorkflow;

public interface IQueue
{	
	Task<Guid> EnqueueAsync(string machineName, Message message, CancellationToken cancellationToken);
	Task<Message?> DequeueAsync(string machineName, CancellationToken cancellationToken);
}
