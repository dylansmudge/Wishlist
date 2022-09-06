using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using Azure.Data.Tables;
using Azure;

namespace TableStorage
{
    public class Favorites
    {

        string AccountName = Environment.GetEnvironmentVariable("AccountName");
        string TableFavorite = Environment.GetEnvironmentVariable("TableFavorites");
        string Uri = Environment.GetEnvironmentVariable("Uri");
        string AccountKey = Environment.GetEnvironmentVariable("AccountKey");
        private readonly TableClient _favoriteTableClient;

        public Favorites() 
        {
            this._favoriteTableClient = 
            new TableClient(new Uri(Uri), 
                TableFavorite, 
                new TableSharedKeyCredential(AccountName, AccountKey));
        }

        [FunctionName("GetFavorites")]
        public async Task<IActionResult> RunGetFavorites(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                Pageable<FavoriteItems> queryResultsFilter = _favoriteTableClient.Query<FavoriteItems>();

                Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");
                
                return new OkObjectResult(queryResultsFilter);

            }
            catch (Exception e)
            {
                log.LogError(e, "Problem loading");
                return new BadRequestObjectResult("There was an issue");
            }
        }

        [FunctionName("GetUserFavorites")]
        public async Task<IActionResult> GetUserFavorites(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string query = req.Query["UserId"];

                Pageable<FavoriteItems> queryResultsFilter = _favoriteTableClient.Query<FavoriteItems>(filter: $"UserId eq {query}");

                Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");
                
                return new OkObjectResult(queryResultsFilter);

            }
            catch (Exception e)
            {
                log.LogError(e, "Problem loading");
                return new BadRequestObjectResult("There was an issue");
            }
        }

        [FunctionName("GetFavoritedItems")]
        public async Task<IActionResult> RunGetFavoritedItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string itemId = req.Query["ItemId"];

                Console.WriteLine($"ItemId {itemId}");

                Pageable<FavoriteItems> queryResultsFilter = _favoriteTableClient.Query<FavoriteItems>(filter: $"ItemId eq {itemId}");

                Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");
                return new OkObjectResult(queryResultsFilter);

            }
            catch (Exception e)
            {
                log.LogError(e, "Problem loading");
                return new BadRequestObjectResult("There was an issue");
            }
        }

        [FunctionName("MakeFavorite")]
        public async Task<IActionResult> RunMakeFavorite(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("Request body is {requestBody}", requestBody);
                FavoriteItems favorites = JsonConvert.DeserializeObject<FavoriteItems>(requestBody);
                Guid obj = Guid.NewGuid();    
                Console.WriteLine("New Guid is " + obj.ToString());  

                await _favoriteTableClient.AddEntityAsync(favorites);

                return new OkObjectResult(favorites);


            }
            catch (Exception e)
            {
                log.LogError(e, "Error with creating item. Check that it does not exist already.");
            }

            return new BadRequestObjectResult("There was an issue");
        }
        [FunctionName("RemoveFavorite")]
        public async Task<IActionResult> RunRemoveFavorite(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "delete", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string PartitionKey = req.Query["PartitionKey"];
                string RowKey = req.Query["RowKey"];
                Console.WriteLine($"query params string is PartitionKey {PartitionKey} RowKey {RowKey}");

                await _favoriteTableClient.DeleteEntityAsync(PartitionKey, RowKey);

                return new OkObjectResult($"Deleted Wishlist item with PartitionKey {PartitionKey}");

            }
            catch (Exception e)
            {
                log.LogError(e, "Error with creating item. Check that it does not exist already.");
            }

            return new BadRequestObjectResult("There was an issue");
        }

    }
}