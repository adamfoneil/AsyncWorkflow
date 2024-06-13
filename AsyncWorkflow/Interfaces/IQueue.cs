using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IQueue
{
    Task<string> EnqueueAsync(string machineName, Message message);
    Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken);
    Task LogFailureAsync(string machineName, Message message, Exception exception, CancellationToken cancellationToken);
}
