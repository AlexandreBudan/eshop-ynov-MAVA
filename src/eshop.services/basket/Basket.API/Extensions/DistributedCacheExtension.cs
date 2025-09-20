using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Extensions;

/// <summary>
/// Provides extension methods for interacting with distributed caching using an <see cref="IDistributedCache"/>.
/// </summary>
public static class DistributedCacheExtensions
{
    /// <summary>
    /// Retrieves an object of type <typeparamref name="T"/> from the distributed cache
    /// using the specified key, deserializing it if found.
    /// </summary>
    /// <typeparam name="T">The type of the object to retrieve from the cache.</typeparam>
    /// <param name="cache">The distributed cache instance to retrieve the item from.</param>
    /// <param name="key">The key of the item to retrieve from the cache.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the deserialized object of type <typeparamref name="T"/> if found, or the default value
    /// of the type if the key does not exist or the value cannot be deserialized.
    /// </returns>
    public static async Task<T?> GetObjectAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default)
    {
        var data = await cache.GetAsync(key, token);
        return data == null ? default : JsonSerializer.Deserialize<T>(data);
    }

    /// <summary>
    /// Stores an object of type <typeparamref name="T"/> in the distributed cache
    /// under the specified key after serializing it.
    /// </summary>
    /// <typeparam name="T">The type of the object to store in the cache.</typeparam>
    /// <param name="cache">The distributed cache instance to store the item in.</param>
    /// <param name="key">The key under which the item should be stored in the cache.</param>
    /// <param name="value">The object of type <typeparamref name="T"/> to be stored in the cache.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation of storing the serialized object
    /// in the cache.
    /// </returns>
    public static Task SetObjectAsync<T>(this IDistributedCache cache, string key, T value,
        CancellationToken token = default)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(value);
        return cache.SetAsync(key, data, token);
    }
}