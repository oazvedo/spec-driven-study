using System.Net;
using AwesomeAssertions;

namespace Template.FunctionalTests;

public class HealthTests(ApiFactory factory)
{
    // CA-10
    [Fact]
    public async Task Health_BancoDisponivel_Retorna200()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("Healthy");
    }
}
