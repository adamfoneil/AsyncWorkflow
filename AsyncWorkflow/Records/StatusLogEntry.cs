namespace AsyncWorkflow.Records;

public record StatusLogEntry<TStatus>(string Key, string Handler, TStatus Value) where TStatus : Enum
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
