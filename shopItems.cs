using System;
using Azure.Data.Tables;
using Azure;

namespace TableStorage
{
    public class WarehouseItems : ITableEntity 
    {
        public string PartitionKey { get; set; }
        public string RowKey {get; set;}
        public string Name { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

        public class Users : ITableEntity 
    {
        public string PartitionKey { get; set; }
        public string RowKey {get; set;}
        public string Name { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class FavoriteItems : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey {get; set;}
        public Int32 UserId {get; set;}
        public Int32 ItemId {get; set;}
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

    }

    public class WishListOptions 
    {
        public string TableItem {get; set;} = "items";
        public string TableUser {get; set;} = "users";
        public string TableFavorites {get; set;} = "favorites";
        public string BlobImage {get; set;} = "images";

    }

        
}
