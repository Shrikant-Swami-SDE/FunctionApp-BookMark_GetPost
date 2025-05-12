using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace az_204_getbookmarks
{
    public static class getbookmarks
    {
        // trigger HTTP  Function app 
        [FunctionName("az_GetBookmarks")]

        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "bookmark/{id}")] HttpRequest req,
                        [CosmosDB(
                databaseName: "func-io-learn-db",
                containerName: "Bookmarks",
                Connection = "CosmosDbConnectionString",
                Id = "{id}",
                PartitionKey = "{id}")] Bookmark bookmark,

            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (bookmark == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(bookmark.url);

        }
    }
    public class Bookmark
    {
        public string id { get; set; }
        public string url { get; set; }
    }
}
