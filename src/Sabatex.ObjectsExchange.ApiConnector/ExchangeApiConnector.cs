using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Sabatex.ObjectsExchange.Models;

namespace Sabatex.ObjectsExchange.ApiConnector;
#nullable enable
public class APIConnector
{
    private static readonly HttpClient client = new(new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
    })
    {
        BaseAddress = new Uri("https://sabatex.francecentral.cloudapp.azure.com/")
    };
    private Token? UserToken = null;
    public Guid? clientId { get; private set; }
    private DateTime? TokenExpiration { get; set; }

    private void UpdateHeaders()
    {
        if (UserToken is not null)
        {
            lock (client.DefaultRequestHeaders)
            {
                if (client.DefaultRequestHeaders.Contains("clientId")) client.DefaultRequestHeaders.Remove("clientId");
                if (client.DefaultRequestHeaders.Contains("apiToken")) client.DefaultRequestHeaders.Remove("apiToken");
                client.DefaultRequestHeaders.Add("clientId", clientId.ToString());
                client.DefaultRequestHeaders.Add("apiToken", UserToken.AccessToken);
            }
        }
    }

    private void SetTokenExpiration()
    {
        if (UserToken is not null) TokenExpiration = DateTime.UtcNow.AddSeconds(UserToken.ExpiresIn);

    }

    public async Task<bool> Login(Guid newClientId, string password)
    {
        // Can do anything before token replacement.
        client.DefaultRequestHeaders.Clear();
        var response = await client.PostAsJsonAsync("api/v0/login", new { clientId = newClientId, password });
        if (response is not null && response.IsSuccessStatusCode)
        {
            UserToken = await response.Content.ReadFromJsonAsync<Token>();
            clientId = newClientId;
            UpdateHeaders();
            SetTokenExpiration();
            OnTokenUpdate?.Invoke(this, UserToken);
            return true;
        }
        else
        {
            UpdateHeaders(); // Return cleaned headers back.
            // Token wasn't updated.
            return false;
        }
    }

    private bool IsTimeExpired() => TokenExpiration is not null && TokenExpiration <= DateTime.UtcNow;

    public async Task<bool> IsTokenAlive()
    {
        if (UserToken is null)
        {
            // OnTokenExpiration?.Invoke(this);
            return false;
        }
        else if (IsTimeExpired())
        {
            if (UserToken.RefreshToken is null)
            {
                UserToken = null;
                clientId = null;
                OnTokenExpiration?.Invoke(this);
                return false;
            }
            else
            {
                var response = client.PostAsJsonAsync("api/v0/RefresToken", new { clientId, password = UserToken.RefreshToken });
                response.Wait();
                if (response is not null && response.Result.IsSuccessStatusCode)
                {
                    UserToken = await response.Result.Content.ReadFromJsonAsync<Token>();
                    UpdateHeaders();
                    SetTokenExpiration();
                    OnTokenUpdate?.Invoke(this, UserToken);
                    return true;
                }
                else
                {
                    UserToken = null;
                    clientId = null;
                    OnTokenExpiration?.Invoke(this);
                    return false;
                }
            }
        }
        return true;
    }

    public event Action<object, Token?>? OnTokenUpdate;

    public event Action<object>? OnTokenExpiration;

    public static APIConnector GetAPIConnector()
    {
        return new APIConnector();
    }

    private APIConnector() { }

    public async Task<long> ApiPostObjectExchangeAsync(PostObject data, Guid destinationId)
    {
        if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
        if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
        client.DefaultRequestHeaders.Add("destinationId", destinationId.ToString());
        var result = await client.PostAsJsonAsync("api/v0/objects", data);
        if(result is not null && result.IsSuccessStatusCode)
        {
            return long.Parse(await result.Content.ReadAsStringAsync());
        }
        else throw new Exception("Unable to get object.");
    }

    public async Task<long> ApiPostObjectExchangeAsync(string objectType, string objectId, string text, DateTime dateStamp, Guid destinationId)
    {
        if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
        if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
        client.DefaultRequestHeaders.Add("destinationId", destinationId.ToString());
        var result = await client.PostAsJsonAsync("api/v0/objects", new { objectType, objectId, text, dateStamp });
        if(result is not null && result.IsSuccessStatusCode)
        {
            return long.Parse(await result.Content.ReadAsStringAsync());
        }
        else throw new Exception("Unable to get object.");
    }

    public async Task<ObjectExchange[]?> ApiGetObjectExchangeAsync()
    {
        if (!(await IsTokenAlive())) return null;
        return await client.GetFromJsonAsync<ObjectExchange[]>("api/v0/objects");
    }

    public async Task<ObjectExchange[]?> ApiGetObjectExchangeAsync(int take = 10)
    {
        if (!(await IsTokenAlive())) return null;
        return await client.GetFromJsonAsync<ObjectExchange[]>($"api/v0/objects?take={take}");
    }

    public async Task<ObjectExchange[]?> ApiGetObjectExchangeAsync(string nodeName, int take = 10)
    {
        if (!(await IsTokenAlive())) return null;
        return await client.GetFromJsonAsync<ObjectExchange[]>($"api/v0/objects?take={take}&nodeName={nodeName}");
    }

    public async Task<bool> ApiDeleteObjectExchangeAsync(long deletedId)
    {
        if (!(await IsTokenAlive())) return false;
        var result = await client.DeleteAsync($"api/v0/objects/{deletedId}");
        return result is not null && result.IsSuccessStatusCode;
    }

    public async Task<bool> ApiDeleteObjectsExchangeAsync()
    {
        if (!(await IsTokenAlive())) return false;
        var got = await client.GetFromJsonAsync<ObjectExchange[]>("api/v0/objects") ?? Array.Empty<ObjectExchange>();
        foreach (var current in got)
        {
            bool temporary = await ApiDeleteObjectExchangeAsync(current.Id);
            if (temporary == false) return false;
        }
        return true;
    }

    public async Task<long> ApiPostQueryAsync(PostObject data, Guid destinationId)
    {
        if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
        var result = await client.PostAsJsonAsync("api/v0/queries", data);
        if(result is not null && result.IsSuccessStatusCode)
        {
            return long.Parse(await result.Content.ReadAsStringAsync());
        }
        else throw new Exception("Unable to get object.");
    }

    public async Task<long> ApiPostQueryAsync(string objectType, string objectId, Guid destinationId)
    {
        if (!(await IsTokenAlive())) throw new Exception("Unable to get object.");
        var result = await client.PostAsJsonAsync("api/v0/queries", new { objectType, objectId });
        if(result is not null && result.IsSuccessStatusCode)
        {
            return long.Parse(await result.Content.ReadAsStringAsync());
        }
        else throw new Exception("Unable to get object.");
    }

    public async Task<QueryObject[]?> ApiGetQueryObjectAsync()
    {
        if (!(await IsTokenAlive())) return null;
        // Ask if line lower has any use in this context.
        if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
        return await client.GetFromJsonAsync<QueryObject[]>($"api/v0/queries");
    }

    public async Task<QueryObject[]?> ApiGetQueryObjectAsync(int take = 10)
    {
        if (!(await IsTokenAlive())) return null;
        // Ask if line lower has any use in this context.
        if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
        return await client.GetFromJsonAsync<QueryObject[]>($"api/v0/queries?take={take}");
    }

    public async Task<QueryObject[]?> ApiGetQueryObjectAsync(string nodeName, int take = 10)
    {
        if (!(await IsTokenAlive())) return null;
        // Ask if line lower has any use in this context.
        if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
        return await client.GetFromJsonAsync<QueryObject[]>($"api/v0/queries?take={take}&nodeName={nodeName}");
    }

    public async Task<bool> ApiDeleteQueryObjectAsync(long deletedId)
    {
        if (!(await IsTokenAlive())) return false;
        // Ask if line lower has any use in this context.
        if (client.DefaultRequestHeaders.Contains("destinationId")) client.DefaultRequestHeaders.Remove("destinationId");
        var result = await client.DeleteAsync($"api/v0/queries/{deletedId}");
        return result is not null && result.IsSuccessStatusCode;
    }
}
