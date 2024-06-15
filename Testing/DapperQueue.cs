using AsyncWorkflow.DapperSqlServer;
using AsyncWorkflow.Records;
using Microsoft.Extensions.Options;
using AsyncWorkflow.Extensions;

namespace Testing
{
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

			var payload = new { Id = 1, Name = "Test", UserName = "nobody.inparticular" };
			var (msgId, timestamp) = await queue.EnqueueAsync(machineName, DefaultHandler, payload);

			var dequeuedMessage = await queue.DequeueAsync(machineName, DefaultHandler, CancellationToken.None);

			Assert.AreEqual(msgId, dequeuedMessage!.Id);
			Assert.AreEqual(timestamp, dequeuedMessage.Timestamp);
		}

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
				LogTable = new QueueSqlOptions.ObjectName("worker", "Error")
			});
		}
	}
}