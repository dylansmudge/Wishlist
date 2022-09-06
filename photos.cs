using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage;
using System.Collections.Generic;

namespace TableStorage
{
    public class Photos
    {

        string AccountName = Environment.GetEnvironmentVariable("AccountName");
        string BlobImage = Environment.GetEnvironmentVariable("BlobImage");
        string Uri = Environment.GetEnvironmentVariable("BlobUri");
        string AccountKey = Environment.GetEnvironmentVariable("AccountKey");

        private readonly BlobContainerClient _photoBlobContainerClient;

        private readonly BlobClient _photoBlobClient;
        public Photos() 
        {
            StorageSharedKeyCredential storageSharedKeyCredential = 
                new StorageSharedKeyCredential(AccountName, AccountKey);

            this._photoBlobContainerClient = 
            new BlobContainerClient(new Uri(Uri), storageSharedKeyCredential);

            this._photoBlobClient = 
            new BlobClient(new Uri(Uri), storageSharedKeyCredential);
        }

    [FunctionName("GetImage")]
        public async Task<IActionResult> GetImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            string name = req.Query["Name"];

            Console.WriteLine($"Name {name}");


            try 
            {
                //string downloadpath = "/Users/dylancarlyle/Documents/Test/DataFiles/Photos/" + name;
                /*BlobDownloadInfo blobdata = await blob.DownloadAsync();
                using(FileStream downloadFileStream = File.OpenWrite(downloadpath)) 
                {  
                    await blobdata.Content.CopyToAsync(downloadFileStream);
                    downloadFileStream.Close();
                }*/
                BlobClient blob = _photoBlobContainerClient.GetBlobClient(name);
                return new OkObjectResult(blob); 

            }
            catch (Exception e)
            {
                log.LogError(e, "Error with listing items.");
            }

            return new BadRequestObjectResult("Trouble showing image");

        }

    [FunctionName("ListImages")]
        public async Task<IActionResult> ListImages(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<String> imageNamesList = new List<String>();
            try 
            {
            
            await foreach (BlobItem blobItem in _photoBlobContainerClient.GetBlobsAsync())
            {
                imageNamesList.Add($"{blobItem.Name}");
            }
            return new OkObjectResult(imageNamesList);
            }
            catch (Exception e)
            {
                log.LogError(e, "Error with listing items.");
            }

            return new BadRequestObjectResult("Trouble showing image");

        }

            
    [FunctionName("UploadImage")]
        public async Task<IActionResult> UploadImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try 
            {
            string content = req.ContentType;
            Console.WriteLine($"Content type {content}");


            BinaryReader reader = new BinaryReader(req.Body);

            byte[] buffer = new byte[req.Body.Length];

            reader.Read(buffer, 0, buffer.Length);

            BinaryData binaryData = new BinaryData(buffer);

            Stream image = binaryData.ToStream();

            BlobHttpHeaders blobHttpHeader = new BlobHttpHeaders();
            string extension = blobHttpHeader.ContentType;
            Console.WriteLine($"Extension {extension}");
            switch (extension.ToLower())
            {
                case "image/jpg":
                case "image/jpeg":
                    blobHttpHeader.ContentType = "image/jpeg";
                    break;
                case "image/png":
                    blobHttpHeader.ContentType = "image/png";
                    break;
                case "image/gif":
                    blobHttpHeader.ContentType = "image/gif";
                    break;
                default:
                    break;
            }
            
            BlobContentInfo blobContentInfo = await _photoBlobClient.UploadAsync(image, blobHttpHeader);

            return new OkObjectResult(image);

            }
            catch (Exception e)
            {
                log.LogError(e, "Error with uploading item.");
            }

            return new BadRequestObjectResult("Trouble uploading image");
        }
    }
}