using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using marking.Common.Models;
using marking.Common.Responses;
using marking.Functions.Entities;

namespace marking.Functions.Functions
{
    public static class markingApi
    {
        [FunctionName(nameof(CreateMarking))]
        public static async Task<IActionResult> CreateMarking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "marking")] HttpRequest req,
        [Table("marking", Connection = "AzureWebJobsStorage")] CloudTable markingTable,
        ILogger log)
        {
            log.LogInformation("Recived a new Marking.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Marking marking = JsonConvert.DeserializeObject<Marking>(requestBody);

            if (string.IsNullOrEmpty(marking?.IdEmpleo.ToString()))
            {
                // respeusta
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a marking."
                });
            }

            MarkingEntity markingEntity = new MarkingEntity
            {
                ETag = "*",
                PartitionKey = "MARKING",
                RowKey = Guid.NewGuid().ToString(),
                IdEmpleo = marking.IdEmpleo,
                DateTimeInOrOut = DateTime.UtcNow,
                Tipo = marking.Tipo,
                consolidated = false,
            };
            TableOperation AddOperation = TableOperation.Insert(markingEntity);
            await markingTable.ExecuteAsync(AddOperation);

            string message = "New marking Stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = markingEntity
            });
        }

        [FunctionName(nameof(UpdateMarking))]
        public static async Task<IActionResult> UpdateMarking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "marking/{id}")] HttpRequest req,
        [Table("marking", Connection = "AzureWebJobsStorage")] CloudTable markingTable,
        string id,
        ILogger log)
        {
            log.LogInformation($"update for todo: {id}, recived");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Marking marking = JsonConvert.DeserializeObject<Marking>(requestBody);

            //validate todo id 
            TableOperation findOperation = TableOperation.Retrieve<MarkingEntity>("MARKING", id);
            TableResult findResult = await markingTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                // respeusta
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Mark not found"
                });
            }

            // update marking
            MarkingEntity markingEntity = (MarkingEntity)findResult.Result;
            markingEntity.consolidated = marking.consolidated;

            if (!string.IsNullOrEmpty(marking?.IdEmpleo.ToString()))
            {
                markingEntity.IdEmpleo = marking.IdEmpleo;
                markingEntity.Tipo = marking.Tipo;
                markingEntity.consolidated = marking.consolidated;
            }

            TableOperation AddOperation = TableOperation.Replace(markingEntity);
            await markingTable.ExecuteAsync(AddOperation);

            string message = $"marking: {id} Update in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = markingEntity
            });
        }

        [FunctionName(nameof(GetAllMarking))]
        public static async Task<IActionResult> GetAllMarking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marking")] HttpRequest req,
        [Table("marking", Connection = "AzureWebJobsStorage")] CloudTable markingTable,
        ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<MarkingEntity> query = new TableQuery<MarkingEntity>();
            TableQuerySegment<MarkingEntity> markings = await markingTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all markings";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = markings
            });
        }

        [FunctionName(nameof(GetMarkingbyId))]
        public static  IActionResult GetMarkingbyId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "marking/{id}")] HttpRequest req,
        [Table("marking", "MARKING", "{id}", Connection = "AzureWebJobsStorage")] MarkingEntity markingEntity,
        string id,
        ILogger log)
        {
            log.LogInformation($"Get Mark by Id: {id} received");

            if (markingEntity == null)
            {
                // respeusta
                return  new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Marking not found"
                });
            }

            string message = $"Marking: {markingEntity.RowKey}, retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = markingEntity
            });
        }

        [FunctionName(nameof(DeleteMarking))]
        public static async Task<IActionResult> DeleteMarking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "marking/{id}")] HttpRequest req,
        [Table("marking", "MARKING", "{id}", Connection = "AzureWebJobsStorage")] MarkingEntity markingEntity,
        [Table("marking", Connection = "AzureWebJobsStorage")] CloudTable markingTable,
        string id,
        ILogger log)
        {
            log.LogInformation($"Delete todo Id: {id} received");

            if (markingEntity == null)
            {
                // respeusta
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "marking not found"
                });
            }

            await markingTable.ExecuteAsync(TableOperation.Delete(markingEntity));
            string message = $"Todo: {markingEntity.RowKey}, deleted";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = markingEntity
            });
        }
    }
}

