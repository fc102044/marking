using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using marking.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace marking.Functions.Functions
{
    public static class ScheduleFunction
    {
        [FunctionName("ScheduleFunction")]
        public static async Task Run(
            [TimerTrigger("0 */2 * * * *")] TimerInfo myTimer,
            [Table("marking", Connection = "AzureWebJobsStorage")] CloudTable markingTable,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
            ILogger log)
        {
            // log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {

                // search markins
                TableQuery<MarkingEntity> query = new TableQuery<MarkingEntity>();
                TableQuerySegment<MarkingEntity> markings = await markingTable.ExecuteQuerySegmentedAsync(query, null);
                // they are passed to a list ordered by the Id and by the date
                List<MarkingEntity> lstMarking = markings.Where(x => x.DateTimeInOrOut.ToString("dd/M/yyyy") == DateTime.UtcNow.ToString("dd/M/yyyy")
                                                                 && x.consolidated == false)
                                                         .OrderBy(x => x.IdEmpleo)
                                                         .ThenBy(x => x.DateTimeInOrOut)
                                                         .ToList();

                if (lstMarking.Count == 0) return;
                // each one is traversed to perform the calculation
                for (int i = 0; i < lstMarking.Count; i++)
                {
                    // f the records are not null and the employees are the same and the marking date is the same, the sum is made
                    if (lstMarking[i] != null && lstMarking[i + 1] != null &&
                        lstMarking[i].IdEmpleo.Equals(lstMarking[i + 1].IdEmpleo)
                    && lstMarking[i].DateTimeInOrOut.ToString("dd/M/yyyy").Equals(lstMarking[i + 1].DateTimeInOrOut.ToString("dd/M/yyyy")))

                    {
                        TimeSpan Diff = lstMarking[i + 1].DateTimeInOrOut - lstMarking[i].DateTimeInOrOut;
                        double diffMinutes = (Math.Truncate(Diff.TotalMinutes * 10000) / 10000);

                        ConsolidatedEntity consolidated = new ConsolidatedEntity
                        {
                            ETag = "*",
                            PartitionKey = "CONSOLIDATED",
                            RowKey = Guid.NewGuid().ToString(),
                            IdWork = lstMarking[i].IdEmpleo,
                            DateConsolidate = DateTime.UtcNow,
                            MinutsWorked = diffMinutes,
                        };
                        // the consolidated is saved 
                        TableOperation AddOperation = TableOperation.Insert(consolidated);
                        await consolidatedTable.ExecuteAsync(AddOperation);

                        // update 1
                        TableOperation findOperation = TableOperation.Retrieve<MarkingEntity>("MARKING", lstMarking[i].RowKey);
                        TableResult findResult = await markingTable.ExecuteAsync(findOperation);
                        MarkingEntity markingEntity = (MarkingEntity)findResult.Result;
                        markingEntity.IdEmpleo = lstMarking[i].IdEmpleo;
                        markingEntity.Tipo = lstMarking[i].Tipo;
                        markingEntity.consolidated = true;

                        TableOperation UpdateOperation = TableOperation.Replace(markingEntity);
                        await markingTable.ExecuteAsync(UpdateOperation);

                        // update 2
                        findOperation = TableOperation.Retrieve<MarkingEntity>("MARKING", lstMarking[i + 1].RowKey);
                        findResult = await markingTable.ExecuteAsync(findOperation);
                        markingEntity = (MarkingEntity)findResult.Result;
                        markingEntity.IdEmpleo = lstMarking[i + 1].IdEmpleo;
                        markingEntity.Tipo = lstMarking[i + 1].Tipo;
                        markingEntity.consolidated = true;
                        UpdateOperation = TableOperation.Replace(markingEntity);
                        await markingTable.ExecuteAsync(UpdateOperation);
                        i++;
                    }
                }

                string message = "Consolidated today successful";
                log.LogInformation(message);
            }
            catch (Exception ex)
            {
                string message = "Failed: " + ex.Message.ToString();
                log.LogInformation(message);
            }
        }
    }
}
