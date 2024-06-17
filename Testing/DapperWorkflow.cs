using AsyncWorkflow.DapperSqlServer;
using AsyncWorkflow.Extensions;
using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Testing;

[TestClass]
public class DapperWorkflow
{
	private const string DbName = "DapperQueue";

	[ClassInitialize]
	public static void Startup(TestContext context)
	{
		LocalDb.EnsureDatabaseExists(DbName);
	}

	[ClassCleanup]
	public static void Cleanup()
	{
		LocalDb.DropDatabase(DbName);
	}

	[TestMethod]
	public void CreateDbObjects()
	{
		var queue = GetQueue();
	}

	[TestMethod]
	public async Task QueueDataAccess()
	{
		var queue = GetQueue();

		const string DefaultHandler = "defaultHandler";
		var machineName = Environment.MachineName;

		var enqueuedPayload = new Payload(232898, "whatever", "nobody.inparticular");
		var (msgId, timestamp) = await queue.EnqueuePayloadAsync(DefaultHandler, enqueuedPayload);

		var dequeuedMessage = await queue.DequeueAsync(machineName, DefaultHandler, CancellationToken.None);
		var dequeuedPayload = JsonSerializer.Deserialize<Payload>(dequeuedMessage!.Payload);

		Assert.AreEqual(msgId, dequeuedMessage!.Id);

		var enqueuedTicks = timestamp.Ticks;
		var dequeuedTicks = dequeuedMessage.Timestamp.Ticks;

		Assert.AreEqual(enqueuedPayload, dequeuedPayload);
		Assert.AreEqual(ToNearestSecond(timestamp), ToNearestSecond(dequeuedMessage.Timestamp));
	}

	[TestMethod]
	public async Task StatusDataAccess()
	{
		var repo = GetStatusRepository();
		await repo.SetAsync(new StatusEntry<string>("2345abc", "Handler1", "Started"));

		var status = await repo.GetAsync("2345abc", "Handler1");
		Assert.IsTrue(status.Status.Equals("Started"));

		await repo.SetAsync(new StatusEntry<string>("2345abc", "Handler1", "Completed"));
		status = await repo.GetAsync("2345abc", "Handler1");
		Assert.IsTrue(status.Status.Equals("Completed"));

		var allStatuses = await repo.GetAsync("2345abc");

	}

	/// <summary>
	/// Truncate milliseconds and ticks to nearest second for more reliable Timestamp comparison
	/// </summary>
	private static DateTime ToNearestSecond(DateTime dateTime) =>
		new(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind);

	private static Queue GetQueue()
	{
		var options = GetOptions();

		var connectionString = LocalDb.GetConnectionString(DbName);
		var dbObjects = new DbObjects(connectionString, options);
		dbObjects.EnsureExists();

		return new Queue(connectionString, dbObjects);
	}

	private static StatusRepository<string> GetStatusRepository()
	{
		var options = GetOptions();
		var connectionString = LocalDb.GetConnectionString(DbName);
		var dbObjects = new DbObjects(connectionString, options);
		dbObjects.EnsureExists();

		return new StatusRepository<string>(connectionString, dbObjects);
	}

	private static IOptions<AsyncWorkflowOptions> GetOptions() => Options.Create(new AsyncWorkflowOptions
	{
		QueueTable = new AsyncWorkflowOptions.ObjectName("worker", "Queue"),
		LogTable = new AsyncWorkflowOptions.ObjectName("worker", "Error"),
		StatusTable = new AsyncWorkflowOptions.ObjectName("worker", "Status")
	});

}

internal record Payload(int Id, string Description, string UserName) : ITrackedPayload<int>
{
	public int Key => Id;
}
