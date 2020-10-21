using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;
using System.Text;

namespace com.sample
{
    public class Alerts
    {
        private string accessToken;
        private string workspaceId;
        private string queryURI;
        private ILogger logger;

        public Alerts(string subscriptionId, string resourceGroup, string workspaceId, string accessToken, string apiVersion, ILogger log)
        {

            this.workspaceId = workspaceId;
            this.accessToken = accessToken;
            this.queryURI = new StringBuilder()
            .Append($"https://management.azure.com/")
            .Append($"subscriptions/{subscriptionId}/")
            .Append($"resourcegroups/{resourceGroup}/")
            .Append("providers/microsoft.operationalinsights/")
            .Append($"workspaces/{workspaceId}/")
            .Append("providers/Microsoft.AlertsManagement/alerts?")
            .Append($"api-version={apiVersion}")
            .ToString();

            this.logger = log;
        }
        public AlertsResponse query(string alertResourceId, string timeRange = "1h", string alertState = "New")
        {
            AlertsResponse alertResponse;
            var paramaterizedURIBuilder = new StringBuilder()
                .Append(this.queryURI)
                .Append($"&alertState={alertState}")
                .Append($"&timeRange={timeRange}")
                .Append("&includeContext=false")
                .Append("&includeEgressConfig=false");
            
            if (alertResourceId != null){
                paramaterizedURIBuilder.Append($"&alertRule={alertResourceId}");
            }
            var paramaterizedURI = paramaterizedURIBuilder
                .ToString();

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(paramaterizedURI);
            webRequest.Headers["Metadata"] = "true";
            webRequest.Method = "GET";
            webRequest.Headers["Content-Type"] = "application/json";
            webRequest.Headers["Authorization"] = $"Bearer {this.accessToken}";

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)webRequest.GetResponse();

                StreamReader streamResponse = new StreamReader(httpResponse.GetResponseStream());
                var stringResponse = streamResponse.ReadToEnd();

                logger.LogInformation("Succeeded in calling Alert API");

                alertResponse = JsonConvert.DeserializeObject<AlertsResponse>(stringResponse);

            }
            catch (Exception e)
            {
                string errorText = String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Query failed");
                this.logger.LogError(errorText);
                throw new Exception();
            }
            return alertResponse;
        }
    }
}
