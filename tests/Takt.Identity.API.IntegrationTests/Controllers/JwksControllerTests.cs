using Newtonsoft.Json.Linq;

namespace Takt.Identity.API.IntegrationTests.Controllers;

[TestFixture]
public class JwksControllerTests : IntegrationTestBase
{
    [Test(Description = "JWKS API should provide keys as expected, otherwise it'll affect other services, if they're not prepared")]
    public async Task ReturnsJwtKeysTest()
    {
        var response = await Fixture.Client.GetAsync("api/v1/.well-known/jwks.json");
        var dataString = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(dataString);
        var key = json["keys"]?.FirstOrDefault();
        
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(json["keys"], Is.Not.Null);
        Assert.That(json["keys"], Has.Count.EqualTo(1));
        
        Assert.That(key, Is.Not.Null);
        Assert.That(key["alg"].Value<string>(), Is.EqualTo("RS256"));
        Assert.That(key["use"].Value<string>(), Is.EqualTo("sig"));
        Assert.That(key["kty"].Value<string>(), Is.EqualTo("RSA"));
        Assert.That(key["kid"].Value<string>(), Is.Not.Null);
        Assert.That(key["n"].Value<string>(), Is.Not.Null);
        Assert.That(key["e"].Value<string>(), Is.Not.Null);
    } 
}