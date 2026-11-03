using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace RestaurantAqa.Tests;

public class UiTestBase : PageTest
{
    private bool _traceStarted;

    [SetUp]
    public async Task StartTrace()
    {
        _traceStarted = false;

        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        _traceStarted = true;
    }

    [TearDown]
    public async Task SaveArtifactsOnFailure()
    {
        if (!_traceStarted)
            return;

        var failed = TestContext.CurrentContext.Result
            .Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;

        if (failed)
        {
            var folder = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "playwright-artifacts"
            );

            Directory.CreateDirectory(folder);

            var testName = TestContext.CurrentContext.Test.Name;

            var safeName = string.Concat(
                testName.Select(c =>
                    Path.GetInvalidFileNameChars().Contains(c)
                        ? '_'
                        : c)
            );

            var filePrefix =
                $"{safeName}-{Guid.NewGuid():N}";

            try
            {
                var screenshotPath = Path.Combine(
                    folder,
                    $"{filePrefix}.png"
                );

                await Page.ScreenshotAsync(new()
                {
                    Path = screenshotPath,
                    FullPage = true
                });

                TestContext.AddTestAttachment(
                    screenshotPath,
                    "Screenshot on failure"
                );
            }
            catch (Exception ex)
            {
                TestContext.WriteLine(
                    $"Screenshot failed: {ex.Message}"
                );
            }

            var tracePath = Path.Combine(
                folder,
                $"{filePrefix}.zip"
            );

            await Context.Tracing.StopAsync(new()
            {
                Path = tracePath
            });

            TestContext.AddTestAttachment(
                tracePath,
                "Playwright trace on failure"
            );
        }
        else
        {
            await Context.Tracing.StopAsync();
        }
    }
}