using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Azure;
using Microsoft.Extensions.Options;

namespace TableStorage
{


    public class LowStock
    {
        string AccountName = Environment.GetEnvironmentVariable("AccountName");
        string TableFavorite = Environment.GetEnvironmentVariable("TableFavorites");
        string TableItem = Environment.GetEnvironmentVariable("TableItem");
        string Uri = Environment.GetEnvironmentVariable("Uri");
        string AccountKey = Environment.GetEnvironmentVariable("AccountKey");
        private readonly TableClient _favoriteTableClient;
        private readonly TableClient _itemTableClient;

        public LowStock(IOptions<WishListOptions> options) 
        {
            string favoriteOptions = options.Value.TableFavorites;

            this._favoriteTableClient = 
            new TableClient(new Uri(Uri), 
                favoriteOptions, 
                new TableSharedKeyCredential(AccountName, AccountKey));

            string itemsOptions = options.Value.TableItem;
            this._itemTableClient = 
            new TableClient(new Uri(Uri), 
                itemsOptions, 
                new TableSharedKeyCredential(AccountName, AccountKey));
        }

            [FunctionName("GetLowStock")]
        public async Task<IActionResult> GetLowStock(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string itemId = req.Query["ItemId"];

                Console.WriteLine($"ItemId {itemId}");


                Pageable<TableEntity> favoritesQuery = _favoriteTableClient.Query<TableEntity>(filter: $"ItemId eq {itemId}");
                int favoritesCount = favoritesQuery.Count();
                Console.WriteLine($"Favorited {favoritesCount} entities.");

                WarehouseItems itemsQuery = await _itemTableClient.GetEntityAsync<WarehouseItems>(
                partitionKey : $"{itemId}",
                rowKey : $"{itemId}");

                int itemQuantity = itemsQuery.Quantity;
                Console.WriteLine($"Quantity is {itemQuantity}");
                

                if (itemQuantity < favoritesCount)
                {
                    return new OkObjectResult($"Low stock");
                }
                else
                    return new OkObjectResult($"There are {itemQuantity - favoritesCount} more items yet to be wishlisted");
            }
            catch (Exception e)
            {
                log.LogError(e, "Problem loading");
                return new BadRequestObjectResult("There was an issue");
            }
        }
    }
}
