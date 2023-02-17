using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using WireMock.Server;
using WireMock.Settings;

namespace Taxjar.Tests
{
    [SetUpFixture]
    public class Bootstrap
    {
        public static TaxjarApi client;
        public static WireMockServer server;
        public static string apiKey;

        [OneTimeSetUp]
        public static void Init()
        {
            server ??= WireMockServer.Start(new WireMockServerSettings
                {
                    Urls = new[] { "http://localhost:9191" }
                });

            var options = GetTestOptions();
            apiKey = options.ApiToken;
            client = new TaxjarApi(apiKey, new { apiUrl = "http://localhost:9191" });
        }

        [OneTimeTearDown]
        public static void Destroy()
        {
            server.Stop();
        }

        private static TestOptions GetTestOptions()
        {
            var json = File.ReadAllText("../../../secrets.json");
            var options = JsonConvert.DeserializeObject<TestOptions>(json);
            return options;
        }

        private class TestOptions
        {
            public string ApiToken { get; set; }
        }
    }
}
