using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace RestaurantAqa.Tests;

public class ApiHealthTests
{
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        var baseUrl =
            Environment.GetEnvironmentVariable("AQA_BASE_URL")
            ?? "https://localhost:7228";

        _client = new HttpClient(
            new HttpClientHandler
            {
                // Лише для локального development-сервера.
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            })
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task HealthEndpointReturnsHealthy()
    {
        using var response = await _client.GetAsync("/api/health");

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK)
        );

        var result = await response.Content
            .ReadFromJsonAsync<HealthResponse>();

        Assert.That(
            result?.Status,
            Is.EqualTo("Healthy")
        );
    }

    private sealed record HealthResponse(string Status);
}