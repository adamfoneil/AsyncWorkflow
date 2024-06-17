using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace AsyncWorkflow.DapperSqlServer;

public class DbObjects(string connectionString, IOptions<AsyncWorkflowOptions> options)
{
	private readonly AsyncWorkflowOptions _options = options.Value;
	private readonly string _connectionString = connectionString;

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
		["Timestamp"] = "datetime NOT NULL",
		["MachineName"] = "nvarchar(100) NOT NULL",
		["Handler"] = "nvarchar(100) NOT NULL",
		["Key"] = $"{_options.StatusTableKeyColumnType} NULL",
		["Payload"] = "nvarchar(max) NOT NULL",
		["Exception"] = "nvarchar(100) NOT NULL",
		["StackTrace"] = "nvarchar(max) NULL"
	});

	public DbTable StatusTable => new(_options.StatusTable, new()
	{
		["Id"] = "bigint IDENTITY(1,1) PRIMARY KEY",
		["Key"] = $"{_options.StatusTableKeyColumnType} NOT NULL",
		["Handler"] = "nvarchar(100) NOT NULL",
		["Status"] = "nvarchar(100) NOT NULL",
		["Timestamp"] = "datetime NOT NULL",
		["Duration"] = "bigint NULL"
	},
	[
		$"CONSTRAINT [U_{_options.StatusTable.Schema}{_options.StatusTable.Name}_KeyHandler] UNIQUE ([Key], [Handler])"
	]);

	public void EnsureExists()
	{
		using var connection = new SqlConnection(_connectionString);
		QueueTable.EnsureExists(connection);
		LogTable.EnsureExists(connection);
		StatusTable.EnsureExists(connection);
	}
}
