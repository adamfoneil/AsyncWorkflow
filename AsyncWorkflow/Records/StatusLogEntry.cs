namespace AsyncWorkflow.Records;

public record StatusLogEntry<TKey, TStatus>(TKey Key, string Handler, TStatus Value) 
    where TStatus : Enum
    where TKey : struct
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
