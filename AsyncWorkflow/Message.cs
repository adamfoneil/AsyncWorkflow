namespace AsyncWorkflow;

public record Message(string Id, string UserName, DateTime Timestamp, string MachineName, string PayloadType, string Payload, int RetryCount = 0);

