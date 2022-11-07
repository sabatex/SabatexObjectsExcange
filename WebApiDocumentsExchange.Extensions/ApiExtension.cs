using System.Net.Http.Json;
using WebApiDocumentsExchange.Models;

namespace WebApiDocumentsExchange.Extensions;

public static class ApiExtension
{
    /// <summary>
    /// login with api 0
    /// </summary>
    /// <param name="nodeName"></param>
    /// <param name="password"></param>
    /// <returns>token</returns>
    public static async Task<bool> Login(this HttpClient client, string nodeName, string password)
    {
        var content = new FormUrlEncodedContent(
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("nodeName",nodeName),
                new KeyValuePair<string, string>("password",password)
            });
        var response = await client.PostAsync("api/v0/login", content);
        if (!response.IsSuccessStatusCode)
            return false;
           //throw new Exception($"The node:{nodeName} is not login");
        var token =  await response.Content.ReadAsStringAsync();
        client.DefaultRequestHeaders.Add("apiToken", token);
        return true;

    }

    #region objects
    public static async Task<long> ApiPostObjectExchangeAsync(this HttpClient client, PostObject obj)
    {
        //client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.PostAsJsonAsync<PostObject>("api/v0/objects", obj);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error post ObjectType:{obj.ObjectId}");
        var result = await response.Content.ReadAsStringAsync();
        return long.Parse(result);
    }
    public static async Task<IEnumerable<ObjectExchange>> ApiGetObjectExchangeAsync(this HttpClient client,string nodeName, int take=10)
    {
        //client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/objects?take={take}&nodeName={nodeName}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
        return  await response.Content.ReadFromJsonAsync<IEnumerable<ObjectExchange>>() ?? new ObjectExchange[] {};
    }
    public static async Task ApiDeleteObjectExchangeAsync(this HttpClient client, long id)
    {
        var response = await client.DeleteAsync($"api/v0/objects/{id}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error delete object with id={id}");
    }
    #endregion

    #region Queries
    public static async Task<long> ApiPostQueryAsync(this HttpClient client, string destination, string objectTypeName,string objectId)
    {
        //client.DefaultRequestHeaders.Add("apiToken", token);
        var content = new QueryedObject(destination, objectTypeName,objectId);
        var response = await client.PostAsJsonAsync<QueryedObject>("api/v0/queries", content);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error post query:{objectTypeName}");
        var result = await response.Content.ReadAsStringAsync();
        return long.Parse(result);
    }
    public static async Task<long> ApiPostQueryAsync(this HttpClient client, string destination, string objectTypeName, Guid objectId)=>await ApiPostQueryAsync(client, destination, objectTypeName,objectId.ToString());

    public static async Task<IEnumerable<QueryObject>> ApiGetQueryObjectAsync(this HttpClient client, string nodeName, int take = 10)
    {
        //client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/queries?take={take}&nodeName={nodeName}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
        return await response.Content.ReadFromJsonAsync<IEnumerable<QueryObject>>() ?? new QueryObject[] { };
    }
    public static async Task ApiDeleteQueryObjectAsync(this HttpClient client, long id)
    {
        //client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.DeleteAsync($"api/v0/queries/{id}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
    }

    #endregion
}
