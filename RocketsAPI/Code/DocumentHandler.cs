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
        #region Variables

        private const string endpointUri = "https://rockets.documents.azure.com:443/";
        private const string primaryKey = "Add key here";
        private const string databaseName = "Rockets";
        private const string collectionName = "Rockets";

        #endregion

        #region Properties

        public static DocumentClient DBClient { get; set; }
        public static Uri DocumentCollectionUri { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Set the database and collection used for queries. Call this in the Startup.Startup() method
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeDocumentClientAsync()
        {
            DBClient = new DocumentClient(new Uri(endpointUri), primaryKey);

            // sets the database for the DocumentClient. Since there is only one database the name does not need to be specified
            await DBClient.GetDatabaseAccountAsync();

            await DBClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collectionName });

            DocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
        }        

        /// <summary>
        /// Inserts or updates a document based on the id found within the JSON object
        /// </summary>
        /// <param name="json">JSON object</param>
        /// <returns></returns>
        public static async Task UpsertDocumentAsync(JObject json)
        {
            if (json["id"] is null)
            {
                await DBClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), json);
            }
            else
            {
                await DBClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), json);
            }
        }

        /// <summary>
        /// Get the documents based on the value associated with the property name
        /// </summary>
        /// <param name="propertyName">Property Name within the JSON documents to search</param>
        /// <param name="value">Value used to find the desired documents</param>
        /// <returns></returns>
        public static async Task<List<dynamic>> GetDocuments(string propertyName, string value)
        {            
            SqlQuerySpec query = CreateQueryBasedOnDataType(propertyName, value);

            List<dynamic> results = new List<dynamic>();            

            using (var queryable = DBClient.CreateDocumentQuery<string>(DocumentCollectionUri, 
                query,
                new FeedOptions { MaxItemCount = 100 }).AsDocumentQuery())
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
                            results.Add(exc.Message);
                        }
                    }
                }
            }

            return results;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// The query must differ between numeric and string values
        /// String queries may use CONTAINS, but cannot for numeric values
        /// An alternative would be to store only string values
        /// </summary>
        /// <param name="propertyName">Property Name within the JSON documents to search</param>
        /// <param name="value">Value used to find the desired documents</param>
        /// <returns></returns>
        private static SqlQuerySpec CreateQueryBasedOnDataType(string propertyName, string value)
        {
            SqlQuerySpec query;
            int numericValue;
            
            if (int.TryParse(value, out numericValue))
            {
                query = new SqlQuerySpec("SELECT * FROM " + collectionName + " c WHERE c." + propertyName + " = @value");
                query.Parameters = new SqlParameterCollection();
                query.Parameters.Add(new SqlParameter("@value", numericValue));
            }
            else
            {
                // convert to lower to ensure all valid results are returned
                query = new SqlQuerySpec("SELECT * FROM " + collectionName + " c WHERE CONTAINS(LOWER(c." + propertyName + "), LOWER(@value))");
                query.Parameters = new SqlParameterCollection();
                query.Parameters.Add(new SqlParameter("@value", value));
            }

            return query;
        }

        #endregion        
    }
}
