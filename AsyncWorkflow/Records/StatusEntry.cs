namespace AsyncWorkflow.Records;

public record StatusEntry<TKey>(TKey Key, string Handler, string Status, DateTime? Timestamp = null) where TKey : notnull;
