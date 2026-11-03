using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NUnit.Framework;

namespace RestaurantAqa.Tests;

[TestFixture]
public class GroupSessionsApiTests
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("AQA_BASE_URL")
        ?? "https://localhost:7228";

    private string OwnerId = null!;
    private string GroupId = null!;
    private string ReservationId = null!;

    private sealed record TestData(
        Guid OwnerId,
        Guid ReservationId,
        Guid GroupReservationId
    );

    private HttpClient _client = null!;

    private static string SessionsUrl(string groupId) =>
        $"/guest/groups/{groupId}/sessions";

    
    [SetUp]
    public async Task SetUp()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };

        var data = await _client.GetFromJsonAsync<TestData>(
            "/aqa/test-data"
        );

        Assert.That(
            data,
            Is.Not.Null,
            "Testing endpoint must return seeded test data."
        );

        OwnerId = data!.OwnerId.ToString();
        ReservationId = data.ReservationId.ToString();
        GroupId = data.GroupReservationId.ToString();
    }


    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    private async Task<HttpResponseMessage> GetSessions(
        string groupId,
        string? userId = null)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            SessionsUrl(groupId)
        );

        if (userId != null)
            request.Headers.Add("X-User-Id", userId);

        return await _client.SendAsync(request);
    }

    
    [Test]
    public async Task OwnerCanViewGroupParticipants()
    {
        using var response = await GetSessions(GroupId, OwnerId);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK)
        );

        using var data = await response.Content
            .ReadFromJsonAsync<JsonDocument>();

        Assert.That(data, Is.Not.Null);

        var root = data!.RootElement;
        var participants = root.GetProperty("participants");

        Assert.That(participants.GetArrayLength(), Is.EqualTo(3));

        var people = participants.EnumerateArray().ToArray();

        var andrii = people.Single(
            p => p.GetProperty("name").GetString() == "Andrii"
        );

        var owner = people.Single(
            p => p.GetProperty("type").GetString() == "Owner"
        );

        // Andrii submitted two Caesar Salads.
        Assert.That(
            andrii.GetProperty("isSubmitted").GetBoolean(),
            Is.True
        );

        var items = andrii.GetProperty("items");

        Assert.That(items.GetArrayLength(), Is.EqualTo(1));

        var dish = items[0];

        Assert.Multiple(() =>
        {
            Assert.That(
                dish.GetProperty("dish").GetProperty("name").GetString(),
                Is.EqualTo("Caesar Salad")
            );

            Assert.That(
                dish.GetProperty("quantity").GetInt32(),
                Is.EqualTo(2)
            );

            Assert.That(
                andrii.GetProperty("total").GetDecimal(),
                Is.EqualTo(360m)
            );

            // Owner has not added any dishes.
            Assert.That(
                owner.GetProperty("items").GetArrayLength(),
                Is.Zero
            );

            Assert.That(
                owner.GetProperty("isSubmitted").GetBoolean(),
                Is.False
            );
        });

        var total = root.GetProperty("total").GetDecimal();

        Assert.That(total, Is.EqualTo(580m));

        var calculatedTotal = people.Sum(
            p => p.GetProperty("total").GetDecimal()
        );

        Assert.That(total, Is.EqualTo(calculatedTotal));
    }

    [Test]
    public async Task AnonymousUserCannotViewGroup()
    {
        using var response = await GetSessions(GroupId);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized)
        );
    }

    [Test]
    public async Task AnotherUserCannotViewGroup()
    {
        var anotherUserId = Guid.NewGuid().ToString();

        using var response = await GetSessions(
            GroupId,
            anotherUserId
        );

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden)
        );
    }

    [Test]
    public async Task NonExistingGroupReturnsNotFound()
    {
        var missingGroupId = Guid.NewGuid().ToString();

        using var response = await GetSessions(
            missingGroupId,
            OwnerId
        );

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound)
        );
    }

    
    [Test]
    public async Task AnonymousUserCannotDeleteParticipant()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/guest/groups/{GroupId}/sessions/{Guid.NewGuid()}"
        );

        using var response = await _client.SendAsync(request);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized)
        );
    }

    [Test]
    public async Task AnotherUserCannotDeleteParticipant()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/guest/groups/{GroupId}/sessions/{Guid.NewGuid()}"
        );

        request.Headers.Add(
            "X-User-Id",
            Guid.NewGuid().ToString()
        );

        using var response = await _client.SendAsync(request);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden)
        );
    }

    [Test]
    public async Task OwnerCannotDeleteNonExistingParticipant()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/guest/groups/{GroupId}/sessions/{Guid.NewGuid()}"
        );

        request.Headers.Add("X-User-Id", OwnerId);

        using var response = await _client.SendAsync(request);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound)
        );
    }

    [Test]
    public async Task DeleteFromNonExistingGroupReturnsNotFound()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/guest/groups/{Guid.NewGuid()}/sessions/{Guid.NewGuid()}"
        );

        request.Headers.Add("X-User-Id", OwnerId);

        using var response = await _client.SendAsync(request);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound)
        );
    }


    [Test]
    public async Task OwnerCannotDeleteOwnSession()
    {
        // 1. Отримуємо поточних учасників групи.
        using var getResponse = await GetSessions(GroupId, OwnerId);

        Assert.That(
            getResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK)
        );

        using var data = await getResponse.Content
            .ReadFromJsonAsync<JsonDocument>();

        Assert.That(data, Is.Not.Null);

        var participants = data!.RootElement
            .GetProperty("participants");

        // 2. Знаходимо Owner за типом, а не за фіксованим ID.
        var owner = participants.EnumerateArray()
            .Single(p =>
                p.GetProperty("type").GetString() == "Owner"
            );

        var ownerSessionId = owner
            .GetProperty("id")
            .GetString();

        Assert.That(ownerSessionId, Is.Not.Null.And.Not.Empty);

        // 3. Пробуємо видалити сесію Owner.
        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/guest/groups/{GroupId}/sessions/{ownerSessionId}"
        );

        deleteRequest.Headers.Add("X-User-Id", OwnerId);

        using var deleteResponse =
            await _client.SendAsync(deleteRequest);

        // 4. Backend повинен заборонити операцію.
        Assert.That(
            deleteResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest)
        );

        // 5. Перевіряємо, що Owner залишився у групі.
        using var afterResponse =
            await GetSessions(GroupId, OwnerId);

        Assert.That(
            afterResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK)
        );

        using var afterData = await afterResponse.Content
            .ReadFromJsonAsync<JsonDocument>();

        Assert.That(afterData, Is.Not.Null);

        var participantsAfter = afterData!.RootElement
            .GetProperty("participants");

        var ownerStillExists = participantsAfter
            .EnumerateArray()
            .Any(p =>
                p.GetProperty("id").GetString() == ownerSessionId
            );

        Assert.That(ownerStillExists, Is.True);
    }

    private async Task<HttpResponseMessage> SubmitGroup(
        string groupId,
        string reservationId,
        string? userId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/group-reservations/{groupId}/submit"
        );

        request.Content = JsonContent.Create(new
        {
            reservationId
        });

        if (userId != null)
            request.Headers.Add("X-User-Id", userId);

        return await _client.SendAsync(request);
    }

    [Test]
    public async Task AnonymousUserCannotSubmitGroup()
    {
        using var response = await SubmitGroup(
            GroupId,
            ReservationId,
            null
        );

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized)
        );
    }

    [Test]
    public async Task AnotherUserCannotSubmitGroup()
    {
        using var response = await SubmitGroup(
            GroupId,
            ReservationId,
            Guid.NewGuid().ToString()
        );

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden)
        );
    }

    [Test]
    public async Task SubmitWithNonExistingGroupReturnsNotFound()
    {
        using var response = await SubmitGroup(
            Guid.NewGuid().ToString(),
            ReservationId,
            OwnerId
        );

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound)
        );
    }

    [Test]
    public async Task SubmitWithNonExistingReservationReturnsNotFound()
    {
        using var response = await SubmitGroup(
            GroupId,
            Guid.NewGuid().ToString(),
            OwnerId
        );

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound)
        );
    }


    [Test]
    public async Task GroupTotalEqualsSumOfParticipantTotals()
    {
        using var response = await GetSessions(GroupId, OwnerId);

        Assert.That(
            response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK)
        );

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );

        var root = json.RootElement;
        var participants = root.GetProperty("participants");

        decimal calculatedTotal = 0;

        foreach (var participant in participants.EnumerateArray())
        {
            decimal calculatedParticipantTotal = 0;

            foreach (var item in participant
                .GetProperty("items").EnumerateArray())
            {
                var price = item.GetProperty("dish")
                    .GetProperty("price").GetDecimal();

                var quantity = item.GetProperty("quantity")
                    .GetInt32();

                calculatedParticipantTotal += price * quantity;
            }

            var participantTotal = participant
                .GetProperty("total").GetDecimal();

            Assert.That(
                participantTotal,
                Is.EqualTo(calculatedParticipantTotal),
                "Participant total must equal the sum of dish prices multiplied by quantities."
            );

            calculatedTotal += participantTotal;
        }

        var groupTotal = root.GetProperty("total").GetDecimal();

        Assert.That(
            groupTotal,
            Is.EqualTo(calculatedTotal),
            "Group total must equal the sum of all participant totals."
        );
    }


    

    [Test]
    public async Task OwnerCanViewEmptyGroup()
    {
        using var createResponse =
            await _client.PostAsync("/aqa/empty-group", null);

        Assert.That(
            createResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK)
        );

        var testData = await createResponse.Content
            .ReadFromJsonAsync<TestData>();

        Assert.That(testData, Is.Not.Null);

        try
        {
            using var response = await GetSessions(
                testData!.GroupReservationId.ToString(),
                testData.OwnerId.ToString()
            );

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK)
            );

            using var document = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync()
            );

            var root = document.RootElement;
            var participants = root.GetProperty("participants");

            Assert.That(
                participants.GetArrayLength(),
                Is.EqualTo(0)
            );

            var total = root.GetProperty("total").GetDecimal();

            Assert.That(total, Is.EqualTo(0m));
        }
        finally
        {
            using var cleanupResponse = await _client.DeleteAsync(
                $"/aqa/empty-group/{testData!.GroupReservationId}"
            );

            Assert.That(
                cleanupResponse.StatusCode,
                Is.EqualTo(HttpStatusCode.NoContent)
            );
        }
    }


}