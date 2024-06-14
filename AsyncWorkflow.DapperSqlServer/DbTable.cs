using Dapper;
using System.Data;

namespace AsyncWorkflow.DapperSqlServer;

internal class DbTable(QueueSqlOptions.ObjectName objectName, Dictionary<string, string> columnDefinitions)
{
	private readonly QueueSqlOptions.ObjectName _objectName = objectName;
	private readonly Dictionary<string, string> _columnDefinitions = columnDefinitions;

	public string FormatedName => $"[{_objectName.Schema}].[{_objectName.Name}]";

	public async Task EnsureExistsAsync(IDbConnection connection)
	{
		if (!await TableExistsAsync(connection))
		{
			await connection.ExecuteAsync(CreateScript());
		}
	}

	public virtual string OutputDeletedColumns => "[deleted].*";

	public IEnumerable<string> Columns => _columnDefinitions.Keys;

	private async Task<bool> TableExistsAsync(IDbConnection connection) =>
		await connection.QuerySingleOrDefaultAsync<int>(
			"SELECT 1 FROM [sys].[tables] WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@name", 
			new { schema = _objectName.Schema, name = _objectName.Name }) == 1;
	
	public string CreateScript() => $"CREATE TABLE {FormatedName} (\r\n{string.Join(",\r\n", _columnDefinitions.Select(kvp => $"[{kvp.Key}] {kvp.Value}"))});";	
}
