namespace AsyncWorkflow.Records;

public record StatusLogEntry<TKey>(TKey Key, string Handler, string Value) where TKey : notnull
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
