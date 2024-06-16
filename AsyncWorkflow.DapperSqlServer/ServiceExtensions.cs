using AsyncWorkflow.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AsyncWorkflow.DapperSqlServer;

public static class ServiceExtensions
{
	public static void AddDapperSqlServerAsyncWorkflow(this IServiceCollection services, string connectionString)
	{
		services.AddSingleton(services => new DbObjects(connectionString, services.GetRequiredService<IOptions<AsyncWorkflowOptions>>()));
		services.AddSingleton<IStatusRepository<string>, StatusRepository<string>>(services => new StatusRepository<string>(connectionString, services.GetRequiredService<DbObjects>()));
		services.AddSingleton<IQueue, Queue>(services => new Queue(connectionString, services.GetRequiredService<DbObjects>()));
	}

	public static void UseDapperSqlServerAsyncWorkflow(this IApplicationBuilder app)
	{
		app.ApplicationServices.GetRequiredService<DbObjects>().EnsureExists();
	}
}
