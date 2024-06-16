namespace AsyncWorkflow.Interfaces;

public interface ITrackedPayload<TKey> where TKey : notnull
{
	TKey Key { get; }
}
