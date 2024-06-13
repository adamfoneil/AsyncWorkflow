namespace AsyncWorkflow;

public interface ITrackingRepository<TStatus> where TStatus : Enum
{
	Task AppendHistoryAsync(StatusLogEntry<TStatus> history);
	Task<IEnumerable<StatusLogEntry<TStatus>>> GetHistoryAsync(string key);
}
