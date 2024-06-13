using AsyncWorkflow.Dapper.Extensions;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Microsoft.Extensions.Options;
using System.Data;

namespace AsyncWorkflow.Dapper;

public class Queue(Func<IDbConnection> connectionFactory, IOptions<QueueSqlOptions> options) : IQueue
{
	private readonly Func<IDbConnection> _connectionFactory = connectionFactory;
	private readonly QueueSqlOptions _options = options.Value;

	public async Task<Message?> DequeueAsync(string machineName, string handler, CancellationToken cancellationToken)
	{
		using var cn = _connectionFactory.Invoke();

		var (result, _) = await cn.DequeueAsync<Message>(
			_options.QueueTable, _options.QueueTableCriteria, 
			new { machineName, handler });

		return result;
	}

	public Task<string> EnqueueAsync(string machineName, Message message)
	{
		throw new NotImplementedException();
	}

	public Task LogFailureAsync(string machineName, Message message, Exception exception, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
