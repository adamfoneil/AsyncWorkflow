using AsyncWorkflow.Interfaces;
using AsyncWorkflow.Records;

namespace AsyncWorkflow.Extensions;

public static class TrackingRepositoryExtensions
{
	public static async Task<IEnumerable<StatusEntry<TKey>>> GetLatestAsync<TKey>(
		this IStatusRepository<TKey> repository, TKey key) 		
		where TKey : struct
	{
		var history = await repository.GetAsync(key);

		return history
			.GroupBy(x => x.Handler)
			.Select(x => x.OrderByDescending(x => x.Timestamp)
			.First());
	}

	public static async Task<Dictionary<string, string>> GetLatestStatusesAsync<TKey>(
		this IStatusRepository<TKey> repository, TKey key) where TKey : struct
	{
		var latest = await GetLatestAsync(repository, key);
		return latest.ToDictionary(item => item.Handler, item => item.Status);
	}
}
