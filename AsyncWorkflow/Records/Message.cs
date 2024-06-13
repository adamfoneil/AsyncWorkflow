namespace AsyncWorkflow.Records;

public record Message(string Handler, string Payload, string? UserName = null)
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
