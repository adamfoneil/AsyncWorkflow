using AsyncWorkflow.DapperSqlServer.Extensions;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace AsyncWorkflow.DapperSqlServer;

public class Queue(string connectionString, IOptions<QueueSqlOptions> options) : IQueue
{	
	private readonly QueueSqlOptions _options = options.Value;
	private readonly string _connectionString = connectionString;

	public async Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken)
	{
		using var cn = new SqlConnection(_connectionString);

		var result = await cn.DequeueAsync<Message>(
			QueueTable.FormatedName, "[MachineName]=@machineName AND [Handler]=@handler", 
			new { machineName, handler }, QueueTable.OutputDeletedColumns);

		return result;
	}	

	public async Task EnqueueAsync(string machineName, Message message)
	{
		using var cn = new SqlConnection(_connectionString);

		await cn.ExecuteAsync(
			@$"INSERT INTO {QueueTable.FormatedName} (
				[MessageId], [Timestamp], [MachineName], [Handler], [Payload]
			) VALUES (
				@id, @timestamp, @machineName, @handler, @payload
			)", new { message.Id, message.Timestamp, machineName, message.Handler, message.Payload });
	}		

	public Task LogFailureAsync(string machineName, Message message, Exception exception, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	private QueueTable QueueTable => new(_options.QueueTable, new()
	{
		["Id"] = "bigint IDENTITY(1,1) PRIMARY KEY",
		["MessageId"] = "nvarchar(36) NOT NULL", // get aliased as Message.Id
		["Timestamp"] = "datetime2 NOT NULL",
		["MachineName"] = "nvarchar(100) NOT NULL",
		["Handler"] = "nvarchar(100) NOT NULL",
		["Payload"] = "nvarchar(max) NULL"
	});

	private DbTable LogTable => new(_options.LogTable, new()
	{
		["Id"] = "bigint IDENTITY(1,1) PRIMARY KEY",
		["MessageId"] = "nvarchar(36) NOT NULL",
		["Timestamp"] = "datetime2 NOT NULL",
		["MachineName"] = "nvarchar(100) NOT NULL",
		["Handler"] = "nvarchar(100) NOT NULL",
		["Payload"] = "nvarchar(max) NULL",
		["Exception"] = "nvarchar(100) NULL",
		["StackTrace"] = "nvarchar(max) NULL"
	});
}

internal class QueueTable(
	QueueSqlOptions.ObjectName objectName,
	Dictionary<string, string> columnDefinitions) : DbTable(objectName, columnDefinitions)
{
	public override string OutputDeletedColumns => string.Join(", ", DeletedColumns);

	private IEnumerable<string> DeletedColumns => Columns
		.Except([ "Id "])
		.Select(col => (col == "MessageId") ? $"[deleted].[MessageId] AS [Id]" : $"[deleted].[{col}]");
}
