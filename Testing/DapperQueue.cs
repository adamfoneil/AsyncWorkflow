using AsyncWorkflow.DapperSqlServer;
using AsyncWorkflow.Extensions;
using AsyncWorkflow.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Testing;

[TestClass]
public class DapperQueue
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
		//LocalDb.DropDatabase(DbName);
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
		var (msgId, timestamp) = await queue.EnqueueAsync(machineName, DefaultHandler, enqueuedPayload);

		var dequeuedMessage = await queue.DequeueAsync(machineName, DefaultHandler, CancellationToken.None);
		var dequeuedPayload = JsonSerializer.Deserialize<Payload>(dequeuedMessage!.Payload);

		Assert.AreEqual(msgId, dequeuedMessage!.Id);

		var enqueuedTicks = timestamp.Ticks;
		var dequeuedTicks = dequeuedMessage.Timestamp.Ticks;

		Assert.AreEqual(enqueuedPayload, dequeuedPayload);
		Assert.AreEqual(ToNearestSecond(timestamp), ToNearestSecond(dequeuedMessage.Timestamp));
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
		var dbObjects = new DbObjects(options);
		dbObjects.EnsureExists(connectionString);

		return new Queue(connectionString, dbObjects);
	}

	private static IOptions<QueueSqlOptions> GetOptions()
	{
		return Options.Create(new QueueSqlOptions
		{
			QueueTable = new QueueSqlOptions.ObjectName("worker", "Queue"),
			LogTable = new QueueSqlOptions.ObjectName("worker", "Error"),
			StatusTable = new QueueSqlOptions.ObjectName("worker", "Status")
		});
	}
}

internal record Payload(int Id, string Description, string UserName) : ITrackedPayload<int>
{	
	public int Key => Id;
}
