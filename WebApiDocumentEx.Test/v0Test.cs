using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WebApiDocumentsExchange.Extensions;
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

    private async Task AddObjectType(string token, string name)
    {
        var objectTypes = await _factory.CreateClient().ApiGetObjectTypesAsync(token);
        if (objectTypes.SingleOrDefault(s => s.Name == name) == null)
        {
            await _factory.CreateClient().ApiPostObjectTypeAsync(token, name);
        }

    }

    private async Task<long[]> SendObjectsAsync(string token, string destination,int count)
    {
        var sendedObjects = new List<long>();
        for (int i = 0; i < 100; i++)
        {
            var r = await _factory.CreateClient().ApiPostObjectExchangeAsync(token, destination,
                new ObjectDescriptorWithBody(Guid.NewGuid().ToString(), "Довідник.Номенклатура", TestConst.TestConstString));
            sendedObjects.Add(r);

        }
        return sendedObjects.ToArray();
    }

    private async Task RandomAnalize(string token, string destination,ObjectExchange objectExchange)
    {
        if (Random.Shared.Next(0, 3) != 0)
        {
            await _factory.CreateClient().ApiMarkObjectExchangeAsync(token, objectExchange.Id);
        }
        else
        {
            await _factory.CreateClient().ApiPostQueryAsync(token, destination, "Довідник.ОдиниціВиміру", Random.Shared.Next(1, 10).ToString());
            await _factory.CreateClient().ApiObjectExchangeUpPriorityAsync(token, objectExchange.Id);
        }

    }

    private async Task<int> ReadObjectsAsync(string token, string destination,int count,bool random=false)
    {
        int result = 0;
        while (result < count)
        {
            int c = count - result;
            int take = c < 10 ? c : 10;
            if (take == 0)
                return result;
            var objects = await _factory.CreateClient().ApiGetObjectExchangeAsync(token, take);
            if (objects.Count() == 0)
                return result;
 
            result = result + objects.Count();
            foreach (var obj in objects)
            {
                if (random)
                    await RandomAnalize(token,destination,obj);
                else
                    await _factory.CreateClient().ApiMarkObjectExchangeAsync(token, obj.Id);
            }
        }
        return result;
    }

    private async Task<int> DeleteObjectsAsync(string token,int count=10)
    {
        var markIsDone = await _factory.CreateClient().ApiGetObjectExchangeIsDoneAsync(token, count);
        Assert.NotNull(markIsDone);
        foreach (var id in markIsDone)
        {
            await _factory.CreateClient().ApiDeleteObjectExchangeAsync(token, id);
        }
        return markIsDone.Count();
    }

    [Fact,Priority(1)]
    public async void TestSimpleExchange()
    {
        // логінимось на сервері
        var senderToken = await _factory.CreateClient().Login("bagel", "1");
        var destinationToken = await _factory.CreateClient().Login("ut", "1");

        // check objectType
        await AddObjectType(senderToken, "Довідник.Номенклатура");
        await AddObjectType(senderToken, "Довідник.ОдиниціВиміру");
        // send 100 objects
        var sendedObjects = await SendObjectsAsync(senderToken,"ut",100);
        await ReadObjectsAsync(destinationToken, "bagel", 100);
        var count = DeleteObjectsAsync(senderToken, 100);
    }

    [Fact, Priority(3)]
    public async void TestExchangeWithQuery()
    {
        // логінимось на сервері
        var senderToken = await _factory.CreateClient().Login("bagel", "1");
        var destinationToken = await _factory.CreateClient().Login("ut", "1");
        var sendedObjects = await SendObjectsAsync(senderToken, "ut", 100);
        var count = await ReadObjectsAsync(destinationToken, "bagel", 100,true);
        count = await DeleteObjectsAsync(senderToken, 100);
        // analize query

        var queries = await _factory.CreateClient().ApiQueryObjectGetAsync(senderToken, 100);
        foreach (var query in queries)
        {
            var r = await _factory.CreateClient().ApiPostObjectExchangeAsync(senderToken, "ut",
                new ObjectDescriptorWithBody(Guid.NewGuid().ToString(), "Довідник.ОдиниціВиміру", TestConst.TestConstString));

            // remove query
            await _factory.CreateClient().ApiQueryObjectDeleteAsync(senderToken, query.Id);
        }

        count = await ReadObjectsAsync(destinationToken, "bagel", 100, true);
        count = await DeleteObjectsAsync(senderToken, 100);
        var s = "";
    }
}
