using Dapper;
using System.Data;

namespace AsyncWorkflow.DapperSqlServer;

public class DbTable(QueueSqlOptions.ObjectName objectName, Dictionary<string, string> columnDefinitions)
{
	private readonly QueueSqlOptions.ObjectName _objectName = objectName;
	private readonly Dictionary<string, string> _columnDefinitions = columnDefinitions;

	public string FormatedName => $"[{_objectName.Schema}].[{_objectName.Name}]";

	public override string ToString() => FormatedName;
	
	public void EnsureExists(IDbConnection connection)
	{
		if (!SchemaExists(connection)) connection.Execute($"CREATE SCHEMA [{_objectName.Schema}]");
		if (!TableExists(connection)) connection.Execute(CreateScript());
	}

	private bool SchemaExists(IDbConnection connection) =>
		connection.QuerySingleOrDefault<int>(
			"SELECT 1 FROM [sys].[schemas] WHERE [name]=@name", new { name = _objectName.Schema }) == 1;	

	public virtual string OutputDeletedColumns => "[deleted].*";

	public IEnumerable<string> Columns => _columnDefinitions.Keys;

	private bool TableExists(IDbConnection connection) =>
		connection.QuerySingleOrDefault<int>(
			"SELECT 1 FROM [sys].[tables] WHERE SCHEMA_NAME([schema_id])=@schema AND [name]=@name", 
			new { _objectName.Schema, _objectName.Name }) == 1;
	
	public string CreateScript() => $"CREATE TABLE {FormatedName} (\r\n{string.Join(",\r\n", _columnDefinitions.Select(kvp => $"[{kvp.Key}] {kvp.Value}"))}\r\n);";	
}
