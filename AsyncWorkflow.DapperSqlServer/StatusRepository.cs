using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AsyncWorkflow.DapperSqlServer;

public class StatusRepository<TKey>(string connectionString, DbObjects dbObjects) : IStatusRepository<TKey> where TKey : notnull
{
	private readonly string _connectionString = connectionString;
	private readonly DbObjects _dbObjects = dbObjects;

	public async Task SetAsync(StatusEntry<TKey> history)
	{
		using var cn = new SqlConnection(_connectionString);
		await SetAsync(cn, history);
	}

	public async Task SetAsync(SqlConnection connection, StatusEntry<TKey> status)
	{
		await connection.ExecuteAsync(
			$@"MERGE INTO {_dbObjects.StatusTable} AS [target]
			USING (
				SELECT @key AS [Key], @handler AS [Handler], @status AS [Status], @duration AS [Duration], @timestamp AS [Timestamp]
			) AS [source]
			ON [target].[Key] = [source].[Key] AND [target].[Handler] = [source].[Handler]
			WHEN MATCHED THEN
				UPDATE SET [target].[Status]=[source].[Status], [target].[Timestamp]=[source].[Timestamp], [target].[Duration]=[source].[Duration]
			WHEN NOT MATCHED THEN
				INSERT ([Key], [Handler], [Status], [Timestamp], [Duration])
				VALUES ([source].[Key], [source].[Handler], [source].[Status], [source].[Timestamp], [source].[Duration]);",
			new { status.Key, status.Handler, status.Status, status.Duration, Timestamp = status.Timestamp ?? DateTime.UtcNow });
	}

	public async Task<IEnumerable<StatusEntry<TKey>>> GetAsync(TKey key)
	{
		using var cn = new SqlConnection(_connectionString);
		return await GetAsync(cn, key);
	}

	public async Task<IEnumerable<StatusEntry<TKey>>> GetAsync(SqlConnection connection, TKey key)
	{
		var results = await connection.QueryAsync<StatusEntryInternal<TKey>>(
			$@"SELECT * FROM {_dbObjects.StatusTable} WHERE [Key]=@key", new { key });

		return results.Select(row => new StatusEntry<TKey>(row.Key, row.Handler, row.Status, row.Duration, row.Timestamp));
	}

	public async Task<StatusEntry<TKey>> GetAsync(TKey key, string handler)
	{
		using var cn = new SqlConnection(_connectionString);
		return await GetAsync(cn, key, handler);
	}

	public async Task<StatusEntry<TKey>> GetAsync(SqlConnection connection, TKey key, string handler)
	{
		var result = await connection.QuerySingleOrDefaultAsync<StatusEntryInternal<TKey>>(
			$@"SELECT * FROM {_dbObjects.StatusTable} WHERE [Key]=@key AND [Handler]=@handler",
			new { key, handler });

		return new(result.Key, result.Handler, result.Status, result.Duration, result.Timestamp);
	}

	public async Task<bool> All(TKey key, string status, params string[] handlers)
	{
		var statuses = (await GetAsync(key)).ToDictionary(row => row.Handler, row => row.Status);
		return handlers.All(handler => statuses.TryGetValue(handler, out var recordedStatus) && status == recordedStatus);
	}
}

internal class StatusEntryInternal<TKey>
{
	public TKey Key { get; set; } = default!;
	public string Handler { get; set; } = null!;
	public string Status { get; set; } = null!;
	public DateTime Timestamp { get; set; }
	public long? Duration { get; set; }
}
