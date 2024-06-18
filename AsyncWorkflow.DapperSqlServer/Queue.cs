using AsyncWorkflow.DapperSqlServer.Extensions;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace AsyncWorkflow.DapperSqlServer;

public class Queue(string connectionString, DbObjects dbObjects) : IQueue
{
	private readonly string _connectionString = connectionString;
	private readonly DbObjects _dbObjects = dbObjects;

	public async Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken)
	{
		using var cn = new SqlConnection(_connectionString);

		var result = await cn.DequeueAsync<MessageInternal>(
			_dbObjects.QueueTable.FormatedName, "[MachineName]=@machineName AND [Handler]=@handler",
			new { machineName, handler });

		if (result is not null)
		{
			return new Message(result.Handler, result.Payload)
			{
				Id = result.MessageId,
				Timestamp = result.Timestamp
			};
		}

		return default;
	}

	public async Task EnqueueAsync(string machineName, Message message)
	{
		using var cn = new SqlConnection(_connectionString);

		await cn.ExecuteAsync(
			@$"INSERT INTO {_dbObjects.QueueTable} (
				[MessageId], [Timestamp], [MachineName], [Handler], [Payload]
			) VALUES (
				@id, @timestamp, @machineName, @handler, @payload
			)", new { message.Id, Timestamp = DateTime.UtcNow, machineName, message.Handler, message.Payload });
	}

	public async Task LogErrorAsync<TKey>(string machineName, string handler, ITrackedPayload<TKey> payload, Exception exception, CancellationToken cancellationToken) where TKey : notnull
	{
		using var cn = new SqlConnection(_connectionString);
		await LogErrorAsync(cn, machineName, handler, payload, exception, cancellationToken);
	}

	public async Task LogErrorAsync<TKey>(SqlConnection connection, string machineName, string handler, ITrackedPayload<TKey> trackedPayload, Exception exception, CancellationToken cancellationToken) where TKey : notnull =>
		await connection.ExecuteAsync(
			$@"INSERT INTO {_dbObjects.ErrorTable} (
				[Timestamp], [MachineName], [Handler], [Key], [Payload], [Exception], [StackTrace]
			) VALUES (
				@timestamp, @machineName, @handler, @key, @payload, @exception, @stackTrace
			)", new 
			{ 
				timestamp = DateTime.UtcNow, 
				machineName, 
				handler, 
				trackedPayload.Key, 
				payload = JsonSerializer.Serialize(trackedPayload),
				exception = ExceptionHeader(exception), 
				exception.StackTrace 
			});

	private static string ExceptionHeader(Exception exception) => $"{exception.GetType().Name}: {exception.Message}";
}

internal class MessageInternal
{
	public string MessageId { get; set; } = null!;
	public DateTime Timestamp { get; set; }
	public string Handler { get; set; } = null!;
	public string Payload { get; set; } = null!;
}
