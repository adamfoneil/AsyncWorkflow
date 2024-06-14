namespace AsyncWorkflow.DapperSqlServer;

public class QueueSqlOptions
{
	public ObjectName QueueTable { get; set; } = default!;
	public ObjectName LogTable { get; set; } = default!;

	public class ObjectName
	{
		public string Schema { get; set; } = default!;
		public string Name { get; set; } = default!;
	}
}
