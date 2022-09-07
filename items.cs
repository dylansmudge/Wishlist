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
using Microsoft.Extensions.Options;

namespace TableStorage
{


    public class Items
    {

        string AccountName = Environment.GetEnvironmentVariable("AccountName");
        string Uri = Environment.GetEnvironmentVariable("Uri");
        string AccountKey = Environment.GetEnvironmentVariable("AccountKey");
        private readonly TableClient _itemTableClient;
        public Items(IOptions<WishListOptions> options) 
        {
            string wishlistOptions = options.Value.TableItem;

            this._itemTableClient = 
            new TableClient(new Uri(Uri), 
                wishlistOptions, 
                new TableSharedKeyCredential(AccountName, AccountKey));
        }


        [FunctionName("PostItem")]
        public async Task<IActionResult> RunPostItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("Request body is {requestBody}", requestBody);


                WarehouseItems warehouseItems = JsonConvert.DeserializeObject<WarehouseItems>(requestBody);
                Guid obj = Guid.NewGuid();    
                Console.WriteLine("New Guid is " + obj.ToString());
                warehouseItems.RowKey = obj.ToString();
                warehouseItems.PartitionKey = obj.ToString();

                await _itemTableClient.AddEntityAsync(warehouseItems);

                return new OkObjectResult(requestBody);

            }
            catch (Exception e)
            {
                log.LogError(e, "Error with creating item. Check that it does not exist already.");
            }

            return new BadRequestObjectResult("There was an issue");
        }
    
        [FunctionName("GetItems")]
        public async Task<IActionResult> RunGetItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                Pageable<WarehouseItems> queryResultsFilter = _itemTableClient.Query<WarehouseItems>();

                Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");
                
                return new OkObjectResult(queryResultsFilter);

            }
            catch (Exception e)
            {
                log.LogError(e, "Problem loading");
                return new BadRequestObjectResult("There was an issue");
            }
        }

                [FunctionName("GetItemsByPartitionKey")]
        public async Task<IActionResult> GetItemsByPartitionKey(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string partitionKey = req.Query["PartitionKey"];
                Pageable<WarehouseItems> getItemsByPartitionKey = _itemTableClient.Query<WarehouseItems>(filter: $"PartitionKey eq {partitionKey}");
                return new OkObjectResult(getItemsByPartitionKey);
            }
            catch (Exception e)
            {
                log.LogError(e, "Problem loading");
                return new BadRequestObjectResult("There was an issue");
            }
        }

        [FunctionName("PostItems")]
        public async Task<IActionResult> RunPostItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("Request body is {requestBody}", requestBody);
                WarehouseItems warehouseItems = JsonConvert.DeserializeObject<WarehouseItems>(requestBody);

                await _itemTableClient.AddEntityAsync(warehouseItems);

                return new OkObjectResult(requestBody);

            }
            catch (Exception e)
            {
                log.LogError(e, "Error with creating item. Check that it does not exist already.");
            }

            return new BadRequestObjectResult("There was an issue");
        }
    }
}
