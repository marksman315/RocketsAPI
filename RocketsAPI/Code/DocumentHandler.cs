using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;

namespace RocketsAPI
{
    public static class DocumentHandler
    {
        private const string endpointUri = "https://rockets.documents.azure.com:443/";
        private const string primaryKey = "Replace this";
        private const string databaseName = "ToDoList";
        private const string collectionName = "Items";

        public static DocumentClient DBClient { get; set; }

        public static async Task InitializeDocumentClientAsync()
        {
            DBClient = new DocumentClient(new Uri(endpointUri), primaryKey);

            await DBClient.GetDatabaseAccountAsync();

            await DBClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collectionName });
        }

        public static async Task CreateFamilyDocumentIfNotExists(JObject value)
        {
            try
            {
                await DBClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, value["Id"].ToString()));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await DBClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), value);                    
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
