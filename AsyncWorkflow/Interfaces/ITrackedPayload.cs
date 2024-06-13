namespace AsyncWorkflow.Interfaces;

public interface ITrackedPayload<TKey> where TKey : struct
{
    TKey Key { get; }
}
