using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IStatusRepository<TKey> where TKey : struct
{
    Task AppendHistoryAsync(StatusLogEntry<TKey> history);
    Task<IEnumerable<StatusLogEntry<TKey>>> GetHistoryAsync(TKey key);
}
