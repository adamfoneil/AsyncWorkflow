using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;

namespace AsyncWorkflow.Extensions;

public static class TrackingRepositoryExtensions
{
	public static async Task<IEnumerable<StatusLogEntry<TKey, TStatus>>> GetLatestAsync<TKey, TStatus>(
		this IStatusRepository<TKey, TStatus> repository, TKey key) 
		where TStatus : Enum
		where TKey : struct
	{
		var history = await repository.GetHistoryAsync(key);

		return history
			.GroupBy(x => x.Handler)
			.Select(x => x.OrderByDescending(x => x.Timestamp)
			.First());
	}

	public static async Task<Dictionary<string, TStatus>> GetLatestStatusesAsync<TKey, TStatus>(
		this IStatusRepository<TKey, TStatus> repository, TKey key) 
		where TStatus : Enum
		where TKey : struct
	{
		var latest = await GetLatestAsync(repository, key);
		return latest.ToDictionary(item => item.Handler, item => item.Value);
	}
}
