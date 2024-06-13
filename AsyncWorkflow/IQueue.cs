﻿namespace AsyncWorkflow;

public interface IQueue
{	
	Task<string> EnqueueAsync(string machineName, Message message);
	Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken);
	Task<Message> LogFailureAsync(string machineName, Message message, Exception exception, CancellationToken cancellationToken);
}
