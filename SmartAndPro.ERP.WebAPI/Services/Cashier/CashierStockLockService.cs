using System.Collections.Concurrent;

namespace SmartAndPro.ERP.WebAPI.Services.Cashier;

/// <summary>
/// Provides per-store, per-item-set exclusive locks to prevent race conditions
/// when multiple cashiers try to sell the same inventory simultaneously.
///
/// Works as an in-process SemaphoreSlim lock. For multi-instance deployments,
/// replace with a distributed lock (e.g. Redis SETNX or SQL sp_getapplock).
/// </summary>
public interface ICashierStockLockService
{
    /// <summary>
    /// Acquires an exclusive lock for the given store + item set.
    /// Dispose the returned token to release the lock.
    /// Throws <see cref="TimeoutException"/> if the lock cannot be acquired within the timeout.
    /// </summary>
    Task<IDisposable> AcquireAsync(Guid storeId, IEnumerable<Guid> itemIds, TimeSpan? timeout = null);
}

public class CashierStockLockService : ICashierStockLockService
{
    // Static dictionary so the same semaphore is reused across scoped instances
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(Guid storeId, IEnumerable<Guid> itemIds, TimeSpan? timeout = null)
    {
        var key = BuildKey(storeId, itemIds);
        var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(15);
        var acquired = await semaphore.WaitAsync(effectiveTimeout);

        if (!acquired)
            throw new TimeoutException(
                $"Could not acquire stock lock for store {storeId} within {effectiveTimeout.TotalSeconds}s. " +
                "Another transaction is processing the same items. Please retry.");

        return new SemaphoreReleaser(semaphore);
    }

    private static string BuildKey(Guid storeId, IEnumerable<Guid> itemIds)
    {
        // Sort item IDs so { A,B } and { B,A } map to the same key
        var sorted = string.Join("_", itemIds.OrderBy(x => x));
        return $"cashier_stock_{storeId}_{sorted}";
    }

    private sealed class SemaphoreReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public SemaphoreReleaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _semaphore.Release();
        }
    }
}
