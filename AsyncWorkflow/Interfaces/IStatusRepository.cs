using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IStatusRepository<TKey> where TKey : notnull
{
    Task SetAsync(StatusEntry<TKey> history);
    Task<StatusEntry<TKey>> GetAsync(TKey key, string handler);
	Task<IEnumerable<StatusEntry<TKey>>> GetAsync(TKey key);
    Task<bool> All(TKey key, string status, params string[] handlers);
}
