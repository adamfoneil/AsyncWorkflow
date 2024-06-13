namespace AsyncWorkflow;

public record Message(string Payload, string? PayloadType = null, string? UserName = null)
{
	public string Id { get; } = Guid.NewGuid().ToString();
	public DateTime Timestamp { get; } = DateTime.UtcNow;	
}
