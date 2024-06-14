using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;

namespace AsyncWorkflow.DapperSqlServer;

public class StatusRepository<TKey> : IStatusRepository<TKey> where TKey : notnull
{
	public Task AppendHistoryAsync(StatusLogEntry<TKey> history)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<StatusLogEntry<TKey>>> GetHistoryAsync(TKey key)
	{
		throw new NotImplementedException();
	}
}
