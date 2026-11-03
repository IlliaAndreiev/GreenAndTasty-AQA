using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace RestaurantAqa.Tests;

[TestFixture]
public class GroupOrderUiTests : PageTest
{
    [Test]
    public async Task OwnerCanViewParticipantsAndTotals()
    {
var frontendUrl =
    Environment.GetEnvironmentVariable("AQA_FRONTEND_URL")
    ?? "http://localhost:4200";

        await Page.GotoAsync(
            $"{frontendUrl.TrimEnd('/')}/reservations/demo-reservation/group-order?demo"
        );

        await Expect(
            Page.GetByRole(
                AriaRole.Heading,
                new() { Name = "Group Pre-Order" }
            )
        ).ToBeVisibleAsync();

        var participants = Page.Locator(
            "section[aria-label='Participants'] li"
        ).Filter(new() { Has = Page.Locator("app-participant-card") });

        await Expect(participants).ToHaveCountAsync(4);

        await Expect(
            Page.GetByText("Still choosing").First
        ).ToBeVisibleAsync();

        await Expect(
            Page.GetByText("No dishes yet")
        ).ToBeVisibleAsync();

        await Expect(
            Page.GetByText("Grand Total")
        ).ToBeVisibleAsync();

        await Expect(
            Page.Locator(".grand-total strong").Last
        ).ToHaveTextAsync("$50.50");
    }
}