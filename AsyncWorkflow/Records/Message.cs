namespace AsyncWorkflow.Records;

public record Message(string Handler, string Payload)
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
}
