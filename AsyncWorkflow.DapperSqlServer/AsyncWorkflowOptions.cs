namespace AsyncWorkflow.DapperSqlServer;

public class AsyncWorkflowOptions
{
	public ObjectName QueueTable { get; set; } = default!;
	public ObjectName ErrorTable { get; set; } = default!;
	public ObjectName StatusTable { get; set; } = default!;
	public string StatusTableKeyColumnType { get; set; } = "nvarchar(36)";

	public record ObjectName(string Schema, string Name);
}
