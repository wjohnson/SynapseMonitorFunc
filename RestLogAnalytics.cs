using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;

namespace com.walgreens
{
    public class RestLogAnalytics
    {
        private string accessToken;
        private string workspaceId;
        private string queryURI;
        private ILogger logger;
        public RestLogAnalytics(string workspaceId, string accessToken, ILogger log)
        {
            this.workspaceId = workspaceId;
            this.accessToken = accessToken;
            queryURI = $"https://api.loganalytics.io/v1/workspaces/{workspaceId}/query";
            this.logger = log;
        }
        public LogAnalyticsResponse query()
        {
            LogAnalyticsResponse eventsQueryResults;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(this.queryURI);
            webRequest.Headers["Metadata"] = "true";
            webRequest.Method = "POST";
            webRequest.Headers["Content-Type"] = "application/json";
            webRequest.Headers["Authorization"] = $"Bearer {this.accessToken}";

            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                string json = "{\"query\":\"AzureActivity | summarize count() by Category\"}";

                streamWriter.Write(json);
            }

            try
            {
                HttpWebResponse laResponse = (HttpWebResponse)webRequest.GetResponse();

                StreamReader streamResponse = new StreamReader(laResponse.GetResponseStream());
                string stringResponse = streamResponse.ReadToEnd();

                eventsQueryResults = JsonConvert.DeserializeObject<LogAnalyticsResponse>(stringResponse);

            }
            catch (Exception e)
            {
                string errorText = String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Query failed");
                this.logger.LogError(errorText);
                throw new Exception();
            }
            return eventsQueryResults;
        }
    }
}
