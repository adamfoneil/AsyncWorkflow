namespace AsyncWorkflow.Dapper;

public class QueueSqlOptions
{
	public string QueueTable { get; set; } = default!;
	public string LogTable { get; set; } = default!;
}
