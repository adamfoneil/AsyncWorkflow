using AsyncWorkflow.Records;

namespace AsyncWorkflow.Interfaces;

public interface IStatusRepository<TKey> where TKey : notnull
{
	/// <summary>
	/// set the status of a given key and handler
	/// </summary>
	Task SetAsync(StatusEntry<TKey> entry);
	/// <summary>
	/// get the entry for a given key and handler
	/// </summary>
	Task<StatusEntry<TKey>?> GetAsync(TKey key, string handler);
	/// <summary>
	/// get all the entries for a given key
	/// </summary>
	Task<IEnumerable<StatusEntry<TKey>>> GetAsync(TKey key);
	/// <summary>
	/// returns true if all entries are in the specified status
	/// </summary>
	Task<bool> AllHaveStatusAsync(TKey key, string status, params string[] handlers);
}
