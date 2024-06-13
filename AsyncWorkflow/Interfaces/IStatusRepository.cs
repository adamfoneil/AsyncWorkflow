using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IStatusRepository<TStatus> where TStatus : Enum
{
    Task AppendHistoryAsync(StatusLogEntry<TStatus> history);
    Task<IEnumerable<StatusLogEntry<TStatus>>> GetHistoryAsync(string key);
}
