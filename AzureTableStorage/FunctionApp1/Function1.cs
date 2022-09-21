using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;
using System.Collections.Concurrent;
using AutoMapper;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            try
            {

                MapperConfiguration mapperConfiguration = new MapperConfiguration(con => con.CreateMap<Product, Product>());
                Mapper mapper = new Mapper(mapperConfiguration);

                // New instance of the TableClient class
                TableServiceClient tableServiceClient = new TableServiceClient(connectionString);

                // New instance of TableClient class referencing the server-side table
                TableClient tableClient = tableServiceClient.GetTableClient(
                    tableName: "adventureworks"
                );

            //    TableClient tableClient1 = new TableClient(connectionString, "adventureworks1");

//                await tableClient1.CreateIfNotExistsAsync();

                await tableClient.CreateIfNotExistsAsync();

                var partitionKey = "gear-surf-surfboards";
                // Create new item using composite key constructor
                var prod1 = new Product()
                {
                    RowKey = "68719518388",
                    PartitionKey = "gear-surf-surfboards",
                    Name = "Ocean Surfboard",
                    Quantity = 8,
                    Sale = true
                };

                // Add new item to server-side table
                await tableClient.AddEntityAsync<Product>(prod1);
                await tableClient.AddEntityAsync<Product>(prod1);
                //  await tableClient.UpsertEntityAsync<Product>(prod1);

                //    prod1.Name = "Edit";

                Product entity = tableClient.GetEntity<Product>(partitionKey, "68719518388");
            //    mapper.Map(entity, )
                entity = new Product { Name = "New", PartitionKey = partitionKey , RowKey = entity.RowKey};

                //await tableClient.UpdateEntityAsync<Product>(entity, ETag.All, TableUpdateMode.Merge);
                await tableClient.UpsertEntityAsync<Product>(entity);

                //   await tableClient.DeleteEntityAsync(partitionKey, "68719518388");

                // Read a single item from container
                var product = await tableClient.GetEntityAsync<Product>(
                    rowKey: "68719518388",
                    partitionKey: "gear-surf-surfboards"
                );
                Console.WriteLine("Single product:");
                Console.WriteLine(product.Value.Name);

                // Read multiple items from container
                var prod2 = new Product()
                {
                    RowKey = "68719518390",
                    PartitionKey = "gear-surf-surfboards",
                    Name = "Sand Surfboard",
                    Quantity = 5,
                    Sale = false
                };

                await tableClient.UpsertEntityAsync<Product>(prod2);

                var products = tableClient.Query<Product>(x => x.PartitionKey == "gear-surf-surfboards");

                Console.WriteLine("Multiple products:");
                foreach (var item in products)
                {
                    Console.WriteLine(item.Name);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return new OkObjectResult("");
        }
    }

    // C# record type for items in the table
    public record Product : ITableEntity
    {
        public string RowKey { get; set; } = default!;

        public string PartitionKey { get; set; } = default!;

        public string Name { get; init; } = default!;

        public int Quantity { get; init; }

        public bool Sale { get; init; }

        public ETag ETag { get; set; } = default!;

        public DateTimeOffset? Timestamp { get; set; } = default!;
    }
}
