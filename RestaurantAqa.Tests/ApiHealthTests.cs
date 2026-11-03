using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace RestaurantAqa.Tests;

public class ApiHealthTests
{
    private readonly HttpClient _client = new(
        new HttpClientHandler
        {
            // Лише для локального development-сервера.
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
    {
        BaseAddress = new Uri("https://localhost:7228")
    };

    [Test]
    public async Task HealthEndpointReturnsHealthy()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var result = await response.Content
            .ReadFromJsonAsync<HealthResponse>();

        Assert.That(result?.Status, Is.EqualTo("Healthy"));
    }

    private sealed record HealthResponse(string Status);
}