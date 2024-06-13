using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IStatusRepository<TKey, TStatus> 
    where TStatus : Enum
    where TKey : struct
{
    Task AppendHistoryAsync(StatusLogEntry<TKey, TStatus> history);
    Task<IEnumerable<StatusLogEntry<TKey, TStatus>>> GetHistoryAsync(TKey key);
}
