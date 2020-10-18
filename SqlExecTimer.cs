using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;

namespace com.sample
{
    public static class SqlExecTimer
    {
        [FunctionName("SqlExecTimer")]
        public static void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var sqlDb = System.Environment.GetEnvironmentVariable("sqlDB");
            var sqlQuery = System.Environment.GetEnvironmentVariable("sqlQuery");
            var tenantid = System.Environment.GetEnvironmentVariable("TENANT_ID");
            var logAnalyticsQuery = System.Environment.GetEnvironmentVariable("logAnalyticsQuery");

            //Get Access Token for SQL DB
            //: API is for Linux Function Apps on Consumption plan
            log.LogInformation("Getting Database Access Token");
            var dbTokenGetter = new RestAccessTokenGetter(log, "https://database.windows.net/", "2017-09-01");
            //Get Access Token for Log Analytics
            log.LogInformation("Getting Log Analytics Access Token");
            var laTokenGetter = new RestAccessTokenGetter(log, "https://api.loganalytics.io/", "2017-09-01");
            log.LogInformation("Succeeded in getting tokens");

            // Call Log Analytics
            int number_of_events = 0;

            log.LogInformation("Querying the Log Analytics Workspace");
            string workspaceId = System.Environment.GetEnvironmentVariable("LOG_ANALYTICS_WORKSPACE_ID");
            var logAnalytics = new RestLogAnalytics(workspaceId, laTokenGetter.accessToken, log);
            number_of_events = logAnalytics.query(logAnalyticsQuery).tables[0].rows.Count;
            log.LogInformation($"Succeeded in querying: {number_of_events} found.");

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
                }
            }
            else{
                log.LogInformation("No events so no query will be executed.");
            }


        }
    }
}
