using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;

namespace RocketsAPI
{
    public static class DocumentHandler
    {
        private const string endpointUri = "https://rockets.documents.azure.com:443/";
        private const string primaryKey = "add your key here";
        private const string databaseName = "Rockets";
        private const string collectionName = "Rockets";

        public static DocumentClient DBClient { get; set; }
        public static Uri DocumentCollectionUri { get; set; }

        public static async Task InitializeDocumentClientAsync()
        {
            DBClient = new DocumentClient(new Uri(endpointUri), primaryKey);

            await DBClient.GetDatabaseAccountAsync();

            await DBClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collectionName });

            DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
        }        

        public static async Task UpsertDocumentAsync(JObject value)
        {
            if (value["id"] is null)
            {
                await DBClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), value);
            }
            else
            {
                await DBClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), value);
            }
        }

        public static async Task<List<dynamic>> GetDocuments(string name, string value)
        {            
            SqlQuerySpec query = CreateQueryBasedOnDataType(name, value);

            List<dynamic> results = new List<dynamic>();            

            using (var queryable = DBClient.CreateDocumentQuery<string>(DocumentCollectionUri, query,
                new FeedOptions { MaxItemCount = 10 }).AsDocumentQuery())
            {
                while (queryable.HasMoreResults)
                {
                    foreach (var b in await queryable.ExecuteNextAsync())
                    {
                        try
                        {
                            results.Add(b);
                        }
                        catch (Exception exc)
                        {
                            var e = exc.Message;
                        }
                    }
                }
            }

            return results;
        }

        private static SqlQuerySpec CreateQueryBasedOnDataType(string name, string value)
        {
            SqlQuerySpec query;
            int numericValue;
            
            if (int.TryParse(value, out numericValue))
            {
                query = new SqlQuerySpec("SELECT * FROM " + collectionName + " c WHERE c." + name + " = @value");
                query.Parameters = new SqlParameterCollection();
                query.Parameters.Add(new SqlParameter("@value", numericValue));
            }
            else
            {
                // convert to lower to ensure all valid results are returned
                query = new SqlQuerySpec("SELECT * FROM " + collectionName + " c WHERE CONTAINS(LOWER(c." + name + "), LOWER(@value))");
                query.Parameters = new SqlParameterCollection();
                query.Parameters.Add(new SqlParameter("@value", value));
            }

            return query;
        }
        
        private static void SetParameterWithCorrectType(SqlQuerySpec query, string value)
        {
            // this prevents quotes from automatically being placed in the query for non-string values
            int numericValue;

            if (int.TryParse(value, out numericValue))
            {
                query.Parameters.Add(new SqlParameter("@value", numericValue));
            }
            else
            {
                query.Parameters.Add(new SqlParameter("@value", value));
            }
        }
    }
}
