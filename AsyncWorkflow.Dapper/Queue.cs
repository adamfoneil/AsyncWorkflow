using AsyncWorkflow.Dapper.Extensions;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Dapper;
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
			_options.QueueTable, "MachineName = @machineName AND Handler = @handler", 
			new { machineName, handler });

		return result;
	}

	public async Task EnqueueAsync(string machineName, Message message)
	{
		using var cn = _connectionFactory.Invoke();

		await cn.ExecuteAsync(
			$"INSERT INTO {_options.QueueTable} (Id, MachineName, Handler, Payload, UserName) VALUES (@id, @machineName, @handler, @payload, @userName)",
			new { message.Id, machineName, message.Handler, message.Payload, message.UserName });
	}

	public Task LogFailureAsync(string machineName, Message message, Exception exception, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
