using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RocketsAPI
{
    public class Program
    {
        private const string EndpointUri = "https://rockets.documents.azure.com:443/";
        private const string PrimaryKey = "htCa5ZMX7rhSghjY0JfyaXNSEim1RsLZxwqWJ82g5pN4RonFa1SkubUFkASb6n2skS2HOuuCVLarRyUxkONFEg==";
        private DocumentClient client;

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();

        public async Task GetStartedDemoAsync()
        {
            this.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);

            await this.client.GetDatabaseAccountAsync();
        }
    }
}
