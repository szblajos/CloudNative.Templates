namespace MyService.Domain.Interfaces;

public interface ICacheService
{
    Task SetResponseAsync<TResponse>(string key, TResponse response, TimeSpan? expiry = null);
    Task<TResponse?> GetResponseAsync<TResponse>(string key);
    Task RemoveResponseAsync(string key);
}
