using NUnit.Framework;
using RestSharp;

namespace MyTestProject.Base
{
    //[TestFixture] 
    public class BaseTest
    {
        protected RestClient Client;

        [SetUp]
        public void SetUpClient()
        {
            var options = new RestClientOptions("https://reqres.in")
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };

            Client = new RestClient(options);
        }

        [TearDown]
        public void DisposeClient()
        {
            Client?.Dispose();
        }
    }
}
