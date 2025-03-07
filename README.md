# RestSharp API Test Automation

This project demonstrates how to perform API test automation in C# using:
- [RestSharp](https://restsharp.dev/) - for making HTTP requests
- [NUnit](https://nunit.org/) – for structuring and running tests
- [FluentAssertions](https://fluentassertions.com/) - for expressive assertions
- [ExtentReports](https://extentreports.com) - for generating rich HTML test reports

We leverage the free reqres.in API as a dummy endpoint to showcase GET, POST, PUT, and error scenarios.
## Prerequisites

1. [.NET 6 SDK or higher](https://dotnet.microsoft.com/en-us/download/dotnet)
2. [Visual Studio](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/) or [Rider](https://www.jetbrains.com/rider/) or any .NET-capable IDE.

## Setup

1. **Clone** or **download** this repository.
   ```bash
   git clone https://github.com/jerrymonkalbert/API-Test-Automation-Reqres.git

2. Open a terminal in the project's root folder(where the .csproj is located).
3. Restore dependencies:
   ```bash
   dotnet restore
4. (Optional) Build the project to ensure everything compiles:
   ```bash
   dotnet build

## Running the Tests
1. From the project root directory, run:
   ```bash
   dotnet test
2. This command will:
   - Build the project
   - Discover and execute all tests marked with [TestFixture] and [Test]
   - Display pass/fail results in the console

## Reporting
We use ExtentReports to generate an HTML report summarizing the test results:
   - The [SetUpFixture] class (TestSetup) initializes ExtentReports once.
   - Each test logs to that report (pass/fail, stack traces, etc.).
   - After test completion, TestSetup flushes the report to a .html file, typically in your bin/Debug/net6.0/ folder (depending on configuration).
   - Report will be saved to: /Path/to/WorkDirectory/MyTestReport.html

## Test Scenarios Covered
1. CreateUser_Success_ShouldReturn201
   POST /api/users – validates a successful 201 response and JSON fields.
2. CreateUser_MissingName_ShouldReturnBadRequest
   Negative scenario: intentionally omits name, expecting 400 Bad Request.
3. GetSingleUser_ShouldReturnOkAndCorrectUser
   GET /api/users/2 – expects a valid user with ID=2 and correct email.
4. GetListOfUsers_ShouldReturnOkAndList
   GET /api/users?page=2 – expects page=2, and a user list in data.
5. UpdateUser_ShouldReturnOkAndUpdatedInfo
   PUT /api/users/2 – simulates updating user data and checks 200 OK.
6. GetNonExistentUser_ShouldReturnNotFound
   GET /api/users/9999 – expects 404 Not Found.
7. Each test includes HTTP status code checks and minimal JSON content validation.

## Customizing Base URL or Environment

 -  Currently, the base URL is hardcoded to https://reqres.in.
 - To run tests against a different environment (e.g., DEV, QA, PROD), you can:
   1. Introduce environment variables.
   2. Use a configuration file (appsettings.json).
   3. Pass a command-line parameter for the base URL and read it in your tests.




