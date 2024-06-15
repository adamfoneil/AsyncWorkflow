using AsyncWorkflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncWorkflow.DapperSqlServer;

public static class ServiceExtensions
{
	public static void AddDapperSqlServerAsyncWorkflow(this IServiceCollection services, string connectionString)
	{		
		services.AddSingleton<DbObjects>();
		services.AddSingleton<IStatusRepository<string>, StatusRepository<string>>(services => new StatusRepository<string>(connectionString, services.GetRequiredService<DbObjects>()));
		services.AddSingleton<IQueue, Queue>();
	}
}
