using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;

namespace AsyncWorkflow.DapperSqlServer;

public class StatusRepository<TKey>(string connectionString, DbObjects dbObjects) : IStatusRepository<TKey> where TKey : notnull
{
	private readonly string _connectionString = connectionString;
	private readonly DbObjects _dbObjects = dbObjects;

	public Task AppendHistoryAsync(StatusLogEntry<TKey> history)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<StatusLogEntry<TKey>>> GetHistoryAsync(TKey key)
	{
		throw new NotImplementedException();
	}
}
