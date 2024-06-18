This is a web queue worker library inspired by a Derek Comartin video [Web-Queue-Worker Architecture Style for Scaling](https://www.youtube.com/watch?v=niAA3bprjNU). I've done some work with background services [BackgroundService.Extensions](https://github.com/adamfoneil/BackgroundService.Extensions), but hadn't done anything with multiple, coordinated workers. I've played a little bit recently with [Coravel](https://github.com/jamesmh/coravel), and it has a nice queueing feature. But as usual, I wanted to try thinking through the problem myself just to get my head around it. Derek mentions a project [Wolverine](https://github.com/JasperFx/wolverine) by name, which I have not looked at.

At the heart of this are some [interfaces](https://github.com/adamfoneil/AsyncWorkflow/tree/master/AsyncWorkflow/Interfaces):
- [IQueue](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/Interfaces/IQueue.cs) defines fundamental operations on a persistent queue of some kind
- [IStatusRepository](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/Interfaces/IStatusRepository.cs) defines a standard way of tracking success, failure, or any kind of process outcome for all workers
- [ITrackedPayload](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/Interfaces/ITrackedPayload.cs) is something you'd implement on a payload model that you need to identify uniquely

The actual `BackgroundService` implementation is [WorkflowBackgroundService](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs). A few key points:
- The first thing to look at is the [ProcessNextMessageAsync](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs#L34) call. This is where the worker is doing its main work within the `BackgroundService.ExecuteAsync` loop, which is running continually [until canceled](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs#L29).
- The outcome of that work recorded with [Status.SetAsync](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs#L43). This captures the duration, status result, and name of the worker (handler) that did the work.
- When work is done, the worker has a chance to define what happens next with the [OnCompletedAsync](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs#L53) method. This is gives us a chance to coordinate sequential and parallel work.
- The dequeing of messages and parsing json payload data happens in [ProcessNextMessageAsync](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs#L68). We'll see the implementation of this shortly. Note that I use the current `MachineName` as an argument. This is the best way I know to ensure that concurrent workers don't do redundant work. By filtering queue reads by machine name, you can have as many different worker machines as you want, and they won't step on each other. The dequeue implementation also really matters here as well. When querying queue messages from a database table (for example), there are some special considerations to prevent duplicate query results that we'll see shortly. The `HandlerName` is really just the class name -- used as a filter to target queue messages to the appropriate handler.
- The [ProcessMessageAsync](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/WorkflowBackgroundService.cs#L80) call is the abstract method where you supply the implementation of the work you're doing.
- There's an [extension method](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow/Extensions/QueueExtensions.cs) `EnqueuePayloadAsync` that performs a json serialization as well as passes the current machine name automatically. This is used throughout the project instead of the interface `EnqueueAsync` method.

# Dapper + SQL Server implementation
You have many options for a queue backing store and data access approach. Dapper is a great choice for working with inline SQL in a type-safe way while having full control of the SQL, so I went with that. I used SQL Server as the backing database because it's easy to get started with.
- This is the [Queue](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow.DapperSqlServer/Queue.cs) implementation.
- I have a couple low-level extension methods relevant here, in particular [DequeueAsync](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow.DapperSqlServer/Extensions/DbConnectionExtensions.cs#L11), which has the special sauce for preventing duplicate reads in concurrent environments -- the `WITH (ROWLOCK, READPAST)` option.
- Here is the [StatusRepository](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow.DapperSqlServer/StatusRepository.cs) implementation.
- The backing tables needed by the implementation are defined in [DbObjects](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow.DapperSqlServer/DbObjects.cs), which works hand-in-hand with [DbTable](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow.DapperSqlServer/DbObjects.cs). You may be tempted to use EF migrations for creating these objects, but I didn't think that would work here.
- There are a couple convenience methods to help with startup code: [ServiceExtensions](https://github.com/adamfoneil/AsyncWorkflow/blob/master/AsyncWorkflow.DapperSqlServer/ServiceExtensions.cs). This is how we ensure the backing tables get created at startup as well as adding necessary services to the DI container.

# Sample API
A very simple implementation is in the [SampleAPI](https://github.com/adamfoneil/AsyncWorkflow/tree/master/SampleAPI) project.
- See [Program.cs](https://github.com/adamfoneil/AsyncWorkflow/tree/master/SampleAPI/Program.cs) to see how an API project would be configured
- See [appsettings.json](https://github.com/adamfoneil/AsyncWorkflow/blob/master/SampleAPI/appsettings.json#L13) configuration
- I added several dummy [workers](https://github.com/adamfoneil/AsyncWorkflow/tree/master/SampleAPI/Workers). All they do is delay a random number of seconds
- You can test this by clicking "Debug" on the `/process` endpoint post in the [.http](https://github.com/adamfoneil/AsyncWorkflow/blob/master/SampleAPI/SampleAPI.http) file. This is simulating a file upload (although there's no actual content being uploaded -- it's just a file name being provided).
- To make it so [Step2](https://github.com/adamfoneil/AsyncWorkflow/blob/master/SampleAPI/Workers/Step2.cs) runs when the 3 `Step1` processes complete, notice that I override the `OnCompleteAsync` method in the `Step1*` workers, [example](https://github.com/adamfoneil/AsyncWorkflow/blob/master/SampleAPI/Workers/Step1A.cs#L19). This is calling [Step2.StartWhenReady](https://github.com/adamfoneil/AsyncWorkflow/blob/master/SampleAPI/Workers/Step2.cs#L22), which is checking to see if all the `Step1` processes are in a "Completed" status before it initiates Step2.

# Testing
To test the inline SQL without needing to run actual workers, I did this here in [DapperWorkflow](https://github.com/adamfoneil/AsyncWorkflow/blob/master/Testing/DapperWorkflow.cs). This was to ensure that the tables created okay and could be queried without error.
