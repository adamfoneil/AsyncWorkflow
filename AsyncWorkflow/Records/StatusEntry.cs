namespace AsyncWorkflow.Records;

public record StatusEntry<TKey>(TKey Key, string Handler, string Status, long? Duration = null, DateTime? Timestamp = null) where TKey : notnull;
