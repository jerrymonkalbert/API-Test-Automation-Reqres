//namespace MyTestProject.Base;

using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;

namespace MyTestProject.Base
{
    [SetUpFixture]
    public class TestSetup
    {
        public static ExtentReports extent;
        private static string extentReportPath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            extentReportPath = Path.Combine(
                TestContext.CurrentContext.WorkDirectory, "MyTestReport.html");

            extent = new ExtentReports();
            var spark = new ExtentSparkReporter(extentReportPath);
            extent.AttachReporter(spark);
            
            TestContext.WriteLine($"Report will be saved to: {extentReportPath}");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            extent.Flush();
            TestContext.WriteLine($"Report generated at: {extentReportPath}");
        }
    }
}
