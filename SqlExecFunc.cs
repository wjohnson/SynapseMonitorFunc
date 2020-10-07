using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SqlFunc
{
    public static class SqlExecFunc
    {
        [FunctionName("SqlExecFunc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // var config = new ConfigurationBuilder()
            //     .SetBasePath(context.FunctionAppDirectory)
            //     .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            //     .AddEnvironmentVariables()
            //     .Build();

            var sqlDb = System.Environment.GetEnvironmentVariable("sqlDB");
            var sqlQuery = System.Environment.GetEnvironmentVariable("sqlQuery");
            var tenantid = System.Environment.GetEnvironmentVariable("tenantid");

            log.LogInformation(sqlDb);
            log.LogInformation(sqlQuery);
            log.LogInformation(tenantid);

            var identityEndpoint = System.Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT");
            var identityHeader = System.Environment.GetEnvironmentVariable("IDENTITY_HEADER");

            log.LogInformation($"Endpoint: {identityEndpoint}");
            log.LogInformation($"Header: {identityHeader}");
            var requestURI = $"{identityEndpoint}?resource=https://database.windows.net/&api-version=2017-09-01";
            log.LogInformation($"requestURI: {requestURI}");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestURI);
            request.Headers["Metadata"] = "true";
            request.Method = "GET";
            request.Headers["secret"] = identityHeader;
            string accessToken = "NOT SET";
            log.LogInformation("We are preparing to get the access token");
            try
            {
                // Call /token endpoint
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Pipe response Stream to a StreamReader, and extract access token
                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                string stringResponse = streamResponse.ReadToEnd();
                log.LogInformation("Response:");
                log.LogInformation(stringResponse);
                Dictionary<string, string> htmlAttributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);
                //Dictionary<string, string> list = (Dictionary<string, string>)JsonConvert.DeserializeObject(stringResponse, typeof(Dictionary<string, string>));
                log.LogInformation("Getting the access token");
                accessToken = htmlAttributes["access_token"];
            }
            catch (Exception e)
            {
                string errorText = String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Acquire token failed");
                log.LogError(errorText);
                return new OkResult();
            }

            // var azureServiceTokenProvider = new AzureServiceTokenProvider();
            // var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com/", tenantid);
            log.LogInformation(accessToken);
            using (SqlConnection connection = new SqlConnection(sqlDb))
            {
                connection.AccessToken = accessToken;

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }

            return new OkResult();
        }
    }
}
