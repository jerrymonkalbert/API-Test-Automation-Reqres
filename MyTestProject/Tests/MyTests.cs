using System.Net;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using RestSharp;
using NUnit.Framework;

namespace MyTestProject.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        public static ExtentReports extent;
        private static string extentReportPath;

        [OneTimeSetUp]
        public void SetUp()
        {
            extentReportPath = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "MyTestReport.html");
            extent = new ExtentReports();
            var spark = new ExtentSparkReporter(extentReportPath);
            extent.AttachReporter(spark);

            TestContext.WriteLine($"Report will be saved to: {extentReportPath}");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            extent.Flush();
            TestContext.WriteLine($"Report generated at: {extentReportPath}");
        }
    }

    [TestFixture]
    public class MyTests
    {
        private ExtentTest _test;
        private RestClientOptions _restClientOptions;

        [SetUp]
        public void Init()
        {
            _test = TestSetup.extent.CreateTest(TestContext.CurrentContext.Test.Name);

            _restClientOptions = new RestClientOptions
            {
                BaseUrl = new Uri("https://reqres.in"),
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };
        }

        [TearDown]
        public void Cleanup()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var stacktrace = TestContext.CurrentContext.Result.StackTrace ?? "";

            if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                _test.Fail("Test Failed");
                _test.Fail("Stacktrace: " + stacktrace);
            }
            else if (status == NUnit.Framework.Interfaces.TestStatus.Passed)
            {
                _test.Pass("Test Passed");
            }
        }
        
        // HELPER METHOD to validate JSON against a given schema file
        private void ValidateJsonSchema(JObject json, string schemaFileName)
        {
            // Build full path to the schema file in the "Schemas" folder
            var schemaPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Schemas", schemaFileName);
            var schemaJson = File.ReadAllText(schemaPath);

            // Parse into a JSchema object
            var schema = JSchema.Parse(schemaJson);

            // Validate
            var isValid = json.IsValid(schema, out IList<string> errors);
            isValid.Should().BeTrue($"Response should match the schema. Errors: {string.Join(", ", errors)}");
        }

        // [Test]
        // public void CreateUser_Success_ShouldReturn201()
        // {
        //     // Arrange
        //     var client = new RestClient(_restClientOptions);
        //     var request = new RestRequest("/api/users", Method.Post);
        //     request.AddHeader("Content-Type", "application/json");
        //
        //     var body = new
        //     {
        //         name = "morpheus",
        //         job = "leader"
        //     };
        //     request.AddJsonBody(body);
        //
        //     // Act
        //     var response = client.Execute(request);
        //
        //     // *** Log to Extent ***
        //     _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
        //     _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
        //
        //     // *** Log to NUnit (Rider console) ***
        //     TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
        //     TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");
        //
        //     // Assert
        //     response.StatusCode.Should().Be(HttpStatusCode.Created,
        //         "successful creation should return 201 Created");
        //
        //     var content = JObject.Parse(response.Content);
        //     content["name"]?.ToString().Should().Be("morpheus");
        //     content["job"]?.ToString().Should().Be("leader");
        //
        //     content["id"].Should().NotBeNull();
        //     content["createdAt"].Should().NotBeNull();
        // }
        
        
        [Test]
        public void CreateUser_Success_ShouldReturn201()
        {
            // Arrange
            var client = new RestClient(_restClientOptions);
            var request = new RestRequest("/api/users", Method.Post);
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                name = "morpheus",
                job = "leader"
            };
            request.AddJsonBody(body);

            // Act
            var response = client.Execute(request);

            // Log to Extent
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
            // Log to NUnit
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");

            // Assert: status code
            response.StatusCode.Should().Be(HttpStatusCode.Created, 
                "successful creation should return 201 Created");

            // 1) Parse JSON
            var content = JObject.Parse(response.Content);

            // 2) Validate fields individually (existing checks)
            content["name"]?.ToString().Should().Be("morpheus");
            content["job"]?.ToString().Should().Be("leader");
            content["id"].Should().NotBeNull();
            content["createdAt"].Should().NotBeNull();

            // 3) **Strict JSON Schema Validation** (new step)
            //    Read the schema file from your project. For example:
            var schemaPath = Path.Combine(TestContext.CurrentContext.TestDirectory, 
                                          "Schemas", 
                                          "CreateUserResponseSchema.json");
            var schemaJson = File.ReadAllText(schemaPath);
            var schema = JSchema.Parse(schemaJson);

            // Validate the JSON object against the schema
            var isValid = content.IsValid(schema, out IList<string> errors);
            isValid.Should().BeTrue($"Response should match the schema. Errors: {string.Join(", ", errors)}");
        }
    

        [Test]
        public void CreateUser_MissingName_ShouldReturnBadRequest()
        {
            // Arrange
            var client = new RestClient(_restClientOptions);
            var request = new RestRequest("/api/users", Method.Post);
            request.AddHeader("Content-Type", "application/json");

            // Missing name field
            var body = new
            {
                job = "leader"
            };
            request.AddJsonBody(body);

            // Act
            var response = client.Execute(request);

            // *** Log to Extent ***
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");

            // *** Log to NUnit (Rider console) ***
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
                "API should return 400 Bad Request when 'name' is missing.");
        }

        [Test]
        public void GetSingleUser_ShouldReturnOkAndCorrectUser()
        {
            // Arrange
            var client = new RestClient(_restClientOptions);
            var request = new RestRequest("/api/users/2", Method.Get);

            // Act
            var response = client.Execute(request);

            // Logging
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");

            // Basic asserts
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Parse JSON
            var content = JObject.Parse(response.Content);

            // Individual field checks (optional)
            content["data"]?["id"]?.ToString().Should().Be("2");
            content["data"]?["email"]?.ToString().Should().Contain("@reqres.in");

            // Strict schema check
            ValidateJsonSchema(content, "GetSingleUserSchema.json");
        }

        [Test]
        public void GetListOfUsers_ShouldReturnOkAndList()
        {
            // Arrange
            var client = new RestClient(_restClientOptions);
            var request = new RestRequest("/api/users?page=2", Method.Get);

            // Act
            var response = client.Execute(request);

            // Logging
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");

            // Basic asserts
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Parse JSON
            var content = JObject.Parse(response.Content);

            // Quick field checks
            content["page"]?.ToString().Should().Be("2");
            content["data"]?.HasValues.Should().BeTrue("there should be a list of user objects for page=2");

            // Strict schema check
            ValidateJsonSchema(content, "GetListOfUsersSchema.json");
        }
        
        [Test]
        public void GetListOfUsers_ShouldReturnOkAndList_PerformanceCheck()
        {
            // Arrange
            var client = new RestClient(_restClientOptions);
            var request = new RestRequest("/api/users?page=2", Method.Get);

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Act
            var response = client.Execute(request);

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Logging
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
            _test.Log(Status.Info, $"[Extent] Response Time: {elapsedMs} ms");
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");
            TestContext.WriteLine($"[NUnit] Response Time: {elapsedMs} ms");

            // Basic asserts
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = JObject.Parse(response.Content);
            content["page"]?.ToString().Should().Be("2");
            content["data"]?.HasValues.Should().BeTrue("there should be a list of user objects for page=2");

            elapsedMs.Should().BeLessThanOrEqualTo(2000, "this endpoint should generally respond within 2 seconds");

            // Strict schema check
            ValidateJsonSchema(content, "GetListOfUsersSchema.json");
        }


        [Test]
        public void UpdateUser_ShouldReturnOkAndUpdatedInfo()
        {
            // Arrange
            var request = new RestRequest("/api/users/2", Method.Put);
            request.AddHeader("Content-Type", "application/json");
            var client = new RestClient(_restClientOptions);

            var body = new
            {
                name = "morpheus",
                job = "zion resident"
            };
            request.AddJsonBody(body);

            // Act
            var response = client.Execute(request);

            // Logging
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");

            // Basic asserts
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = JObject.Parse(response.Content);
            content["name"]?.ToString().Should().Be("morpheus");
            content["job"]?.ToString().Should().Be("zion resident");
            content["updatedAt"].Should().NotBeNull("API usually returns an updatedAt field");

            // Strict schema check
            ValidateJsonSchema(content, "UpdateUserSchema.json");
        }

        [Test]
        public void GetNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var request = new RestRequest("/api/users/9999", Method.Get);
            var client = new RestClient(_restClientOptions);

            // Act
            var response = client.Execute(request);

            // Logging
            _test.Log(Status.Info, $"[Extent] Response Status: {response.StatusCode}");
            _test.Log(Status.Info, $"[Extent] Response Content: {response.Content}");
            TestContext.WriteLine($"[NUnit] Response Status: {response.StatusCode}");
            TestContext.WriteLine($"[NUnit] Response Content: {response.Content}");

            // Basic asserts
            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                "non-existent user should return 404 Not Found on reqres.in");

            // Parse JSON
            var content = JObject.Parse(response.Content);

            // Strict schema check for an empty object
            ValidateJsonSchema(content, "UserNotFoundSchema.json");
        }
    }
}
