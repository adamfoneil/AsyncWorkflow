using Dapper;
using System.Data;

namespace AsyncWorkflow.DapperSqlServer.Extensions;

public static class DbConnectionExtensions
{
	/// <summary>
	/// gets the next TMessage from a specified database table, ensuring that concurrent readers don't get the same row
	/// </summary>
	public static async Task<TMessage?> DequeueAsync<TMessage>(this IDbConnection connection, string tableName, string? criteria = null, object? parameters = null)
	{
		string sql = $"DELETE TOP (1) FROM {tableName} WITH (ROWLOCK, READPAST) OUTPUT [deleted].*";
		if (!string.IsNullOrEmpty(criteria)) sql += $" WHERE {criteria}";

		TMessage message = await connection.QuerySingleOrDefaultAsync<TMessage>(sql, parameters);

		return message;
	}

	/// <summary>
	/// dequeue all TMessages from a table, doing some work on each TMessage
	/// </summary>    
	public static async Task ProcessQueueAsync<TMessage>(this IDbConnection connection, string tableName, Func<TMessage?, Task> task, CancellationToken cancellationToken, string? crtieria = null, object? parameters = null)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			var result = await DequeueAsync<TMessage?>(connection, tableName, crtieria, parameters);
			if (result is not null)
			{
				await task.Invoke(result);
			}			
		}
	}
}