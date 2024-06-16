namespace AsyncWorkflow.Records;

public record Message(string Handler, string Payload)
{
	public string Id { get; init; } = Guid.NewGuid().ToString();
	public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
