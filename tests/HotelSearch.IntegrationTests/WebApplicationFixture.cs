using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelSearch.IntegrationTests;

public class WebApplicationFixture : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;

    public WebApplicationFixture(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
