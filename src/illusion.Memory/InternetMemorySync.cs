using System.Net.Http.Json;

namespace illusion.Memory;

/// <summary>
/// Minimal pull/push bridge between local user memory and an internet endpoint.
/// The caller owns authentication headers on the supplied HttpClient.
/// </summary>
public sealed class InternetMemorySync
{
    private readonly HttpClient _httpClient;
    private readonly UserPrivatePlaneStore _localStore;

    public InternetMemorySync(HttpClient httpClient, UserPrivatePlaneStore localStore)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _localStore = localStore ?? throw new ArgumentNullException(nameof(localStore));
    }

    public async Task PushAsync<T>(Uri endpoint, string ns, string key, string passphrase, CancellationToken cancellationToken = default)
    {
        var value = _localStore.Get<T>(ns, key, passphrase);
        using var response = await _httpClient.PostAsJsonAsync(endpoint, new MemoryEnvelope<T>(ns, key, value), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task PullAsync<T>(Uri endpoint, string ns, string key, string passphrase, CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(endpoint, $"?ns={Uri.EscapeDataString(ns)}&key={Uri.EscapeDataString(key)}");
        var envelope = await _httpClient.GetFromJsonAsync<MemoryEnvelope<T>>(requestUri, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidDataException("internet memory endpoint returned no record");
        _localStore.Put(envelope.Namespace, envelope.Key, envelope.Value, passphrase);
    }

    public sealed record MemoryEnvelope<T>(string Namespace, string Key, T Value);
}
