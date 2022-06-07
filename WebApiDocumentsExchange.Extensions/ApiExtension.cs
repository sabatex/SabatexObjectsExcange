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
    public static async Task<string> Login(this HttpClient client, string nodeName, string password)
    {
        var content = new FormUrlEncodedContent(
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("nodeName",nodeName),
                new KeyValuePair<string, string>("password",password)
            });
        var response = await client.PostAsync("api/v0/login", content);
        if (!response.IsSuccessStatusCode)
           throw new Exception($"The node:{nodeName} is not login");
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<IEnumerable<ClientNode>> ApiGetNodesAsync(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/nodes");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get nodes");
        return await response.Content.ReadFromJsonAsync<IEnumerable<ClientNode>>();
    }



    public static async Task<ObjectType?> GetObjectTypeByNameAsync(this HttpClient client, string token, string name)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/ObjectTypeByName/?name={name}");
        if (response.StatusCode==System.Net.HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error read ObjectType:{name} is not login");
        return await response.Content.ReadFromJsonAsync<ObjectType>();
    }

    #region ObjectTypes
    public static async Task<ObjectType[]> ApiGetObjectTypesAsync(this HttpClient client, string token, string nodeName)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/objecttypes?node={nodeName}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return new ObjectType[] {};

        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error read ObjectType:{nodeName} is not login");
        return await response.Content.ReadFromJsonAsync<ObjectType[]>() ?? new ObjectType[] {};
    }

    public static async Task<ObjectType[]> ApiGetObjectTypesAsync(this HttpClient client, string token, int id) =>
       await ApiGetObjectTypesAsync(client,token,id.ToString());
  
    public static async Task<ObjectType[]> ApiGetObjectTypesAsync(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/objecttypes");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return new ObjectType[] { };

        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error read ObjectType ");
        return await response.Content.ReadFromJsonAsync<ObjectType[]>() ?? new ObjectType[] { };
    }

    public static async Task<ObjectType?> ApiPostObjectTypeAsync(this HttpClient client, string token, string objectTypeName)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var content = new FormUrlEncodedContent(
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("objectTypeName",objectTypeName)
            });

        var response = await client.PostAsync("api/v0/objecttypes", content);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error post ObjectTypeName:{objectTypeName}");
        return await response.Content.ReadFromJsonAsync<ObjectType>();
    }

    #endregion

    #region objects
    public static async Task<long> ApiPostObjectExchangeAsync(this HttpClient client, string token,PostObject obj)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.PostAsJsonAsync<PostObject>("api/v0/objects", obj);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error post ObjectType:{obj.ObjectId}");
        var result = await response.Content.ReadAsStringAsync();
        return long.Parse(result);
    }
    public static async Task<IEnumerable<ObjectExchange>> ApiGetObjectExchangeAsync(this HttpClient client, string token, int take=10)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/objects?take={take}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
        return  await response.Content.ReadFromJsonAsync<IEnumerable<ObjectExchange>>() ?? new ObjectExchange[] {};
    }

    public static async Task ApiMarkObjectExchangeAsync(this HttpClient client, string token, long id)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var content = new FormUrlEncodedContent(
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("id",id.ToString()),
            });

        //client.DefaultRequestHeaders.Add("destinationName", destination);
        var response = await client.PostAsync($"api/v0/objects/done/{id}", null);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The mark done object with id={id}");
    }

    public static async Task ApiDeleteObjectExchangeAsync(this HttpClient client, string token, long id)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var content = new FormUrlEncodedContent(
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("id",id.ToString()),
            });

        //client.DefaultRequestHeaders.Add("destinationName", destination);
        var response = await client.DeleteAsync($"api/v0/objects/done/{id}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error delete object with id={id}");
    }

    public static async Task<long[]> ApiGetObjectExchangeIsDoneAsync(this HttpClient client, string token,int take = 10)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/objects/done?take={take}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
        return await response.Content.ReadFromJsonAsync<long[]>() ?? new long[] { };

    }
    public static async Task ApiObjectExchangeUpPriorityAsync(this HttpClient client, string token, long id)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var content = new FormUrlEncodedContent(
            new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("id",id.ToString()),
            });

        //client.DefaultRequestHeaders.Add("destinationName", destination);
        var response = await client.PostAsync($"api/v0/objects/Priority/{id}", null);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The dont up priority object with id={id}");
    }

    #endregion

    #region Queries
    public static async Task<long> ApiPostQueryAsync(this HttpClient client, string token, string destination, string objectTypeName,string objectId)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var content = new QueryedObject(destination,objectId, objectTypeName);
        var response = await client.PostAsJsonAsync<QueryedObject>("api/v0/queries", content);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error post query:{objectTypeName}");
        var result = await response.Content.ReadAsStringAsync();
        return long.Parse(result);
    }
    public static async Task<IEnumerable<QueryObject>> ApiQueryObjectGetAsync(this HttpClient client, string token, int take = 10)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.GetAsync($"api/v0/queries?take={take}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
        return await response.Content.ReadFromJsonAsync<IEnumerable<QueryObject>>() ?? new QueryObject[] { };
    }
    public static async Task ApiQueryObjectDeleteAsync(this HttpClient client, string token, long id)
    {
        client.DefaultRequestHeaders.Add("apiToken", token);
        var response = await client.DeleteAsync($"api/v0/queries/{id}");
        if (!response.IsSuccessStatusCode)
            throw new Exception($"The error get  object ");
    }

    #endregion
}
