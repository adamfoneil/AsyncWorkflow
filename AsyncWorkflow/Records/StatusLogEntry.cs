namespace AsyncWorkflow.Records;

public record StatusLogEntry<TKey>(TKey Key, string Handler, string Value) where TKey : struct
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
