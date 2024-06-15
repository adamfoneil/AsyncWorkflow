using Microsoft.Data.SqlClient;
using System.Data;

namespace Testing;

internal static class LocalDb
{
	internal static string GetConnectionString(string databaseName) =>
		$"Server=(localdb)\\mssqllocaldb;Database={databaseName};Integrated Security=true;MultipleActiveResultSets=true";

	internal static SqlConnection GetConnection(string databaseName)
	{
		SqlConnection result = new(GetConnectionString(databaseName));
		result.Open();
		return result;
	}		

	internal static void EnsureDatabaseExists(string databaseName)
	{
		using var cn = GetConnection("master");		
		if (!DatabaseExists(cn, databaseName))
		{
			using var cmd = new SqlCommand($"CREATE DATABASE [{databaseName}]", cn);
			cmd.ExecuteNonQuery();
		}
	}

	internal static void DropDatabase(string databaseName)
	{
		using var cn = GetConnection("master");
		if (DatabaseExists(cn, databaseName))
		{
			using var cmd = new SqlCommand(
				@$"USE MASTER;
				ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
				DROP DATABASE [{databaseName}];", cn);
			cmd.ExecuteNonQuery();
		}
	}

	private static bool DatabaseExists(SqlConnection cn, string databaseName)
	{
		using var cmd = new SqlCommand("SELECT 1 FROM [sys].[databases] WHERE [Name]=@name", cn);
		var param = new SqlParameter("name", SqlDbType.NVarChar, 50);
		param.Value = databaseName;
		cmd.Parameters.Add(param);
		var result = cmd.ExecuteScalar();
		return result?.Equals(1) ?? false;		
	}
}
