# Green & Tasty — AQA

**Selected direction:** Direction 1 — Group Pre-Order  
**Current target:** Level 1

## Architecture

- **Backend:** ASP.NET Core Web API, Entity Framework Core, SQLite
- **Frontend:** Angular
- **Automation:** C#, NUnit, Playwright
- **CI:** GitHub Actions

The project uses API tests for group preorder behavior and a UI smoke test for the Angular group-order page.

**Important:** The current UI test uses frontend demo data. It does not yet verify end-to-end integration with the real Backend.

## Run API tests

Start the Backend in the `Testing` environment on port `5065`.

Then run:

```powershell
$env:AQA_BASE_URL = "http://127.0.0.1:5065"
dotnet test .\RestaurantAqa.Tests\RestaurantAqa.Tests.csproj
```

## Run the empty-group test

```powershell
dotnet test .\RestaurantAqa.Tests\RestaurantAqa.Tests.csproj --filter "FullyQualifiedName~OwnerCanViewEmptyGroup"
```

## Test data

The Backend exposes test-only fixture endpoints in the `Testing` environment. The empty-group test creates its own reservation and group and deletes them in a `finally` block.

## CI

GitHub Actions builds the application, starts the Backend and Angular frontend, runs automated tests, and uploads test reports and diagnostic artifacts.

## Current coverage and limitations

Covered scenarios include API health, group-session retrieval, authorization and negative cases, and an empty group with zero total.

Remaining Level 1 work includes verifying that the group total updates when a participant changes their cart, expanding role-based access checks, and validating frontend behavior against the real Backend.