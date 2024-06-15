namespace AsyncWorkflow.DapperSqlServer;

public class QueueSqlOptions
{
	public ObjectName QueueTable { get; set; } = default!;
	public ObjectName LogTable { get; set; } = default!;

	public record ObjectName(string Schema, string Name);	
}
