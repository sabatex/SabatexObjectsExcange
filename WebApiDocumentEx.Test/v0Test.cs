using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebApiDocumentsExchange.Models;
using Xunit;

namespace WebApiDocumentEx.Test;
[TestCaseOrderer("WebApiDocumentEx.Test.PriorityOrderer", "WebApiDocumentEx.Test")]
public class v0Test : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public v0Test(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }


    async Task<string> login(string nodeName,string password)
    {
        var client = _factory.CreateClient();
        var content = new FormUrlEncodedContent(
            
            new KeyValuePair<string,string>[] 
            { 
                new KeyValuePair<string, string>("nodeName",nodeName),
                new KeyValuePair<string, string>("password",password)
            }
            );
        var response = await client.PostAsync("api/v0/login", content);
        Assert.True(response.IsSuccessStatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    [Fact, Priority(1)]
    public async void Login()
    {
        await login("bagel", "1");
        await login("1c", "1");
    }


    [Fact,Priority(2)]
    public async void TestSimpleExchange()
    {
        var _senderToken = await login("bagel", "1");
        var _destinationToken = await login("1c", "1");

        var obj = new PostObject
        {
            DestinationNode = "1c",
            ObjectId = Guid.NewGuid(),
            ObjectType = "test type",
            ObjectJson = "{ }"

        };
        
        // send object
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _senderToken);
        var response = await client.PostAsJsonAsync<PostObject>("api/v0", obj);
        Assert.True(response.IsSuccessStatusCode);

        // read objects
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _destinationToken);
        response = await client.GetAsync($"api/v0/objects?take={50}");
        Assert.True(response.IsSuccessStatusCode);
        var objects = await response.Content.ReadFromJsonAsync<ObjectExchange[]>();
        Assert.NotNull(objects);
        Assert.True(objects.Length > 0);
        var sendObject = objects.Single(s => s.ObjectId == obj.ObjectId);

        //mark resived
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _destinationToken);
        response = await client.PostAsJsonAsync<Guid>("api/v0/MarkResived", sendObject.Id);
        Assert.True(response.IsSuccessStatusCode);

        //delete resived
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _senderToken);
        response = await client.PostAsJsonAsync<Guid[]>("api/v0/delete", new Guid[] { sendObject.Id });
        Assert.True(response.IsSuccessStatusCode);


    }

    [Fact, Priority(3)]
    public async void TestExchangeWithQuery()
    {
        var _senderToken = await login("bagel", "1");
        var _destinationToken = await login("1c", "1");

        var obj = new PostObject
        {
            DestinationNode = "1c",
            ObjectId = Guid.NewGuid(),
            ObjectType = "test type",
            ObjectJson = "{ }"

        };

        // send object (1)
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _senderToken);
        var response = await client.PostAsJsonAsync<PostObject>("api/v0", obj);
        Assert.True(response.IsSuccessStatusCode);

        // read objects
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _destinationToken);
        response = await client.GetAsync($"api/v0/objects?take={50}");
        Assert.True(response.IsSuccessStatusCode);
        var objects = await response.Content.ReadFromJsonAsync<ObjectExchange[]>();
        Assert.NotNull(objects);
        Assert.True(objects.Length > 0);
        var sendObject = objects.Single(s => s.ObjectId == obj.ObjectId);

        // need query
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _destinationToken);
        response = await client.PostAsJsonAsync<QueryObjects>("api/v0/queries", new QueryObjects
        {
            objectId = sendObject.Id,
            ObjectsJson = "query items from sender"
        });
        Assert.True(response.IsSuccessStatusCode);

        // sender read query
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _senderToken);
        response = await client.GetAsync($"api/v0/queries?take={50}");
        Assert.True(response.IsSuccessStatusCode);
        var queryObjects = await response.Content.ReadFromJsonAsync<QueryObjects[]>();
        Assert.NotNull(queryObjects);
        Assert.True(queryObjects.Length > 0);





        //mark resived
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _destinationToken);
        response = await client.PostAsJsonAsync<Guid>("api/v0/MarkResived", sendObject.Id);
        Assert.True(response.IsSuccessStatusCode);

        //delete resived
        client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("apiToken", _senderToken);
        response = await client.PostAsJsonAsync<Guid[]>("api/v0/delete", new Guid[] { sendObject.Id });
        Assert.True(response.IsSuccessStatusCode);


    }
}
