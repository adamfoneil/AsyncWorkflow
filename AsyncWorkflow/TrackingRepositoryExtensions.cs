namespace AsyncWorkflow;

public static class TrackingRepositoryExtensions
{
	public static async Task<IEnumerable<StatusLogEntry<TStatus>>> GetLatestAsync<TStatus>(this ITrackingRepository<TStatus> repository, string key) where TStatus : Enum
	{
		var history = await repository.GetHistoryAsync(key);
		return history
			.GroupBy(x => x.Handler)
			.Select(x => x.OrderByDescending(x => x.Timestamp)
			.First());
	}
}
