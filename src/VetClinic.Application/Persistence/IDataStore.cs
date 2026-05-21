namespace VetClinic.Application.Persistence;

/// <summary>
/// Contract for asynchronous data persistence.
/// Supports loading and saving collections of entities to/from storage.
/// </summary>
/// <typeparam name="T">Type of entity being persisted</typeparam>
public interface IDataStore<T>
{
    /// <summary>
    /// Asynchronously load all items from storage
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Collection of loaded items; empty collection if storage is empty or doesn't exist</returns>
    Task<IReadOnlyCollection<T>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously save items to storage
    /// </summary>
    /// <param name="items">Collection of items to persist</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <exception cref="InvalidOperationException">Thrown when save operation fails</exception>
    Task SaveAsync(IReadOnlyCollection<T> items, CancellationToken cancellationToken = default);
}
