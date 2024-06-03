using System;
//using System.Reflection.Metadata.Ecma335;
#if NET6_0_OR_GREATER
using System.Threading.Tasks;
#endif
namespace Sabatex.ObjectsExchange.ApiConnector.Tests
{
    public class Tests
    {
        private Guid clientId = new Guid("891afa3a-abb3-4972-a0d7-2d8bfd77a323");
        private Guid destinationId = new Guid("c46d4ff7-358e-4bb4-b3d8-6e0ff814c6d4");
        ExchangeApiConnector? apiConnector;
        string refreshToken = "";
        DateTime expiredToken;

        [SetUp]
        public void Setup()
        {
#if NET6_0_OR_GREATER
            apiConnector = new ExchangeApiConnector("",clientId,destinationId,"",DateTime.UtcNow,false,
                async ()=> await Task.FromResult("Dsfewc1243"),
                async ()=>await Task.FromResult(refreshToken),
                async (token, expiredToken) => { this.expiredToken = expiredToken; refreshToken = token.RefreshToken;await Task.Yield(); }
                );
#else
            apiConnector= new ExchangeApiConnector("",clientId,destinationId,"",DateTime.UtcNow,false,
                ()=>"Dsfewc1243",
                ()=>refreshToken,
                (token, expiredToken) => { this.expiredToken = expiredToken; refreshToken = token.RefreshToken; }
                );
#endif
        }
#if NET6_0_OR_GREATER
        [Test,Order(5)]
        

        public async Task TestPostObject()
        {
            try
            {

                if (apiConnector == null)
                    throw new ArgumentNullException(nameof(apiConnector));
                await apiConnector.PostObjectAsync("testType", "testId", "test TEXT");
                
            }
            catch (Exception ex) { Assert.Fail(ex.Message); }
            Assert.Pass();
        }

        [Test, Order(2)]


        public async Task TestGetObject()
        {
            try
            {

                if (apiConnector == null)
                    throw new ArgumentNullException(nameof(apiConnector));
                var objects = await apiConnector.GetObjectsAsync();
                foreach (var obj in objects)
                {
                    await apiConnector.DeleteObjectAsync(obj.Id);
                }

            }
            catch (Exception ex) { Assert.Fail(ex.Message); }
            Assert.Pass();
        }


#else
       [Test,Order(5)]
        

        public void TestPostObject()
        {
            try
            {

                if (apiConnector == null)
                    throw new ArgumentNullException(nameof(apiConnector));
                apiConnector.PostObject("testType", "testId", "test TEXT");
                
            }
            catch (Exception ex) { Assert.Fail(ex.Message); }
            Assert.Pass();
        }

#endif
    }

}