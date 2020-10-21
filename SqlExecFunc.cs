using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using com.sample;

namespace SqlFunc
{
    public static class SqlExecFunc
    {
        [FunctionName("SqlExecFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var sqlDb = System.Environment.GetEnvironmentVariable("sqlDB");
            var sqlQuery = System.Environment.GetEnvironmentVariable("sqlQuery");
            string workspaceName = System.Environment.GetEnvironmentVariable("LOG_ANALYTICS_WORKSPACE_NAME");
            string subscriptionId = System.Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
            string resourceGroup = System.Environment.GetEnvironmentVariable("RESOURCE_GROUP");
            string alertRuleId = System.Environment.GetEnvironmentVariable("ALERT_RULE_ID");

            log.LogInformation("Getting Database Access Token");
            var dbTokenGetter = new RestAccessTokenGetter(log, "https://database.windows.net/", "2017-09-01");
            //Get Access Token for Log Analytics
            log.LogInformation("Getting Azure Monitor Access Token");
            var monitorTokenGetter = new RestAccessTokenGetter(log, "https://management.azure.com/", "2017-09-01");

            int number_of_events = 0;

            log.LogInformation("Querying the Azure Monitor Workspace");

            var monitorResults = new Alerts(
                subscriptionId,
                resourceGroup,
                workspaceName,
                monitorTokenGetter.accessToken,
                "2019-03-01",
                log
            );

            var results = monitorResults.query(alertRuleId);

            number_of_events = results.value.Count;
            log.LogInformation($"Found {number_of_events} security events");

            if (number_of_events > 0)
            {

                try
                {
                    // Call SQL DB
                    using (SqlConnection connection = new SqlConnection(sqlDb))
                    {
                        connection.AccessToken = dbTokenGetter.accessToken;

                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            connection.Open();

                            command.ExecuteNonQuery();
                            log.LogInformation("Completed the query");
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogError("Failed to execute SQL query");
                    string errorText = String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Failed to Query SQL DB");
                    log.LogError(errorText);
                    throw new Exception(errorText);
                }
            }
            else
            {
                log.LogInformation("No events so no query will be executed.");
            }
            return new OkObjectResult(results);
        }

    }
}
