using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using marking.Common.Responses;
using marking.Functions.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace marking.Functions.Functions
{
    public static class consolidatedApi
    {

        [FunctionName(nameof(GetMarkingConsolidatedByDate))]
        public static async Task<IActionResult> GetMarkingConsolidatedByDate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Consolidate/{DateConsolidate}")] HttpRequest req,
        [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
        string DateConsolidate,
        ILogger log)
        {
            log.LogInformation($"Get consolidated by date: {DateConsolidate} received");

            if (consolidatedTable == null)
            {
                // respeusta
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Marking not found"
                });
            }

            // search markins
            TableQuery<ConsolidatedEntity> query = new TableQuery<ConsolidatedEntity>();
            TableQuerySegment<ConsolidatedEntity> consolidated = await consolidatedTable.ExecuteQuerySegmentedAsync(query, null);

            var lstConsolidate = consolidated.Where(x => x.DateConsolidate.ToString("dd/MM/yyyy") == DateConsolidate.Replace("-", "/"))
                                             .GroupBy(x => new
                                             {
                                                 x.IdWork,
                                                 x.DateConsolidate.Year,
                                                 x.DateConsolidate.Month,
                                                 x.DateConsolidate.Day
                                             }).Select(x => new
                                             {
                                                 Id = x.Key.IdWork,
                                                 TotalTime = x.Sum(s => s.MinutsWorked) / 60,
                                                 Date = x.Key.Year + "/" + x.Key.Month + "/" + x.Key.Day
                                             }).ToList();
            string Marks = "[";
            for (int i = 0; i < lstConsolidate.Count; i++)
            {
                Marks += "{IdWork: '" + lstConsolidate[i].Id + "'," +
                               "TotalTime: '" + (Math.Truncate(lstConsolidate[i].TotalTime * 100) / 100).ToString().Replace(",",":") + " hours '," +
                               "Date: '" + lstConsolidate[i].Date + "'},";
            }
            Marks += "]";
            string message = $"Get {lstConsolidate.Count} items";
            log.LogInformation(message);
            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = JsonConvert.DeserializeObject(Marks)
            }); ;
        }
    }
}
