namespace AsyncWorkflow.Dapper;

public class QueueSqlOptions
{
	public string QueueTable { get; set; } = default!;
	/// <summary>
	/// shouold be a WHERE clause that includes the machineName and handler
	/// </summary>
	public string QueueTableCriteria { get; set; } = default!;
	public string LogTable { get; set; } = default!;
}
