using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Microsoft.Playwright;

namespace RestaurantAqa.Tests;

public class FirstE2ETests : UiTestBase
{
    [Test]
    public async Task UserCanOpenTodoApp()
    {
        await Page.GotoAsync("https://demo.playwright.dev/todomvc/");

        await Expect(Page).ToHaveTitleAsync("React • TodoMVC");

        await Expect(
            Page.GetByPlaceholder("What needs to be done?")
        ).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task UserCanAddAndCompleteTodo()
    {
        await Page.GotoAsync("https://demo.playwright.dev/todomvc/");

        var input = Page.GetByPlaceholder("What needs to be done?");

        await input.FillAsync("Learn Playwright");
        await input.PressAsync("Enter");

        var todo = Page.GetByTestId("todo-item")
            .Filter(new() { HasText = "Learn Playwright" });

        await Expect(todo).ToBeVisibleAsync();

        await todo.GetByRole(AriaRole.Checkbox).CheckAsync();

        await Expect(todo).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("completed")
        );
    }
}