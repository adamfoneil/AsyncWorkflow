using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace AsyncWorkflow.DapperSqlServer;

public class DbObjects(IOptions<QueueSqlOptions> options)
{
	private readonly QueueSqlOptions _options = options.Value;

	public DbTable QueueTable => new(_options.QueueTable, new()
	{
		["Id"] = "bigint IDENTITY(1,1) PRIMARY KEY",
		["MessageId"] = "nvarchar(36) NOT NULL", // gets aliased as Message.Id
		["Timestamp"] = "datetime NOT NULL",
		["MachineName"] = "nvarchar(100) NOT NULL",
		["Handler"] = "nvarchar(100) NOT NULL",
		["Payload"] = "nvarchar(max) NULL"
	});

	public DbTable LogTable => new(_options.LogTable, new()
	{
		["Id"] = "bigint IDENTITY(1,1) PRIMARY KEY",
		["MessageId"] = "nvarchar(36) NOT NULL",
		["Timestamp"] = "datetime NOT NULL",
		["MachineName"] = "nvarchar(100) NOT NULL",
		["Handler"] = "nvarchar(100) NOT NULL",
		["Payload"] = "nvarchar(max) NULL",
		["Exception"] = "nvarchar(100) NULL",
		["StackTrace"] = "nvarchar(max) NULL"
	});	

	public void EnsureExists(string connectionString)
	{
		using var connection = new SqlConnection(connectionString);
		QueueTable.EnsureExists(connection);
		LogTable.EnsureExists(connection);
	}
}
