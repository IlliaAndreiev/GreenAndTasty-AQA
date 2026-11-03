# Green & Tasty — AQA

Automated testing project for the Green & Tasty
group pre-order application.

## Tech Stack

- C#
- .NET 9
- NUnit
- Microsoft Playwright
- ASP.NET Core API
- GitHub Actions

## Test Coverage

### Level 1 — Group Pre-order Overview

- Owner can view group participants
- Participant dishes and quantities
- Participant totals and group total
- Submitted and pending sessions
- Unauthorized access (401)
- Forbidden access (403)
- Non-existing resources (404)
- Owner session deletion protection

### Additional API Tests

- Submit authorization and validation
- Guest session deletion validation
- API health check

### UI Tests

- TodoMVC page loading
- Adding and completing a todo item

The TodoMVC tests use a public demo application
to demonstrate Playwright UI automation.

## Run Tests Locally

Start the Green & Tasty backend on:

https://localhost:7228

Then run:

```powershell
dotnet test
```

## Generate TRX Report

```powershell
dotnet test --logger "trx;LogFileName=aqa-results.trx" --results-directory TestResults
```

## Playwright Artifacts

Screenshots and traces are saved automatically
when a UI test fails.

## CI

GitHub Actions currently runs the Playwright
UI smoke tests.

Backend API tests require a running local API
and seeded test data.