using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetPost_Bookmarks_Bindings
{
    public static class GetPostBookmarks
    {
        //  trigger post Http function app
        [FunctionName("BookmarkFunction")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bookmarks/{id}")] HttpRequest req,
        [CosmosDB(
            databaseName: "func-io-learn-db",
            containerName: "Bookmarks",
            Connection = "az204cosmodbnosql_DOCUMENTDB",
            Id = "{id}",
            PartitionKey = "{id}")] Bookmark existingBookmark,
        [CosmosDB(
            databaseName: "func-io-learn-db",
            containerName: "Bookmarks",
            Connection = "az204cosmodbnosql_DOCUMENTDB")] IAsyncCollector<Bookmark> newBookmarkCollector,
        [Blob("bookmarks/{id}", FileAccess.Write, Connection = "azuresrikantstorageac_STORAGE")] Stream blobStream,
        ILogger log)
        {
            log.LogInformation("Processing POST request...");

            // Parse the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Bookmark>(requestBody);

            if (data == null)
            {
                log.LogError("Request body is null or invalid.");
                return new BadRequestObjectResult("Invalid request body.");
            }

            // Log the ID from the request body
            log.LogInformation($"Request Body ID: {data.Id}");

            // Check if bookmark exists in Cosmos DB
            if (existingBookmark != null)
            {
                // Return response indicating that the bookmark already exists
                return new ConflictObjectResult("Bookmark already exists.");
            }

            // Add the new bookmark to Cosmos DB
            await newBookmarkCollector.AddAsync(data);

            // Add the new bookmark to Blob Storage
            using (var writer = new StreamWriter(blobStream))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(data));
            }

            return new CreatedResult($"bookmarks/{data.Id}", "Bookmark added successfully.");
        }

        public class Bookmark
        {
            public string Id { get; set; }
            public string Url { get; set; }
        }
    }
}