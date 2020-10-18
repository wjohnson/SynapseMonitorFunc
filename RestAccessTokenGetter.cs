using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace com.walgreens
{
    public class RestAccessTokenGetter
    {
        public string accessToken { get; }
        public RestAccessTokenGetter(ILogger log, string resource, string apiVersion)
        {
            var envVars = System.Environment.GetEnvironmentVariables();
            HttpWebRequest request;
            if (envVars.Contains("IDENTITY_ENDPOINT") & envVars.Contains("IDENTITY_HEADER"))
            {
                log.LogInformation("Found MSI environment variables, using them to call MSI for access token.");
                var identityEndpoint = System.Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT");
                var identityHeader = System.Environment.GetEnvironmentVariable("IDENTITY_HEADER");
                var requestURI = $"{identityEndpoint}?resource={resource}&api-version={apiVersion}";
                request = (HttpWebRequest)WebRequest.Create(requestURI);
                if (apiVersion == "2017-09-01")
                {
                    //This api version didn't handle things consistently
                    request.Headers["secret"] = identityHeader;
                }
                else
                {
                    //This is the consistent way of doing this.
                    request.Headers["X-IDENTITY-HEADER"] = identityHeader;
                }
                request.Method = "POST";

            }
            else
            {
                // This is NOT using the Managed Identity, so pull from env variables
                var tenantId = System.Environment.GetEnvironmentVariable("TENANT_ID");
                var clientId = System.Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = System.Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var oAuthURL = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";
                request = (HttpWebRequest)WebRequest.Create(oAuthURL);
                request.Method = "POST";
                var payload = new StringBuilder()
                    .Append($"resource={resource}&")
                    .Append($"client_id={clientId}&")
                    .Append("grant_type=client_credentials&")
                    .Append($"client_secret={clientSecret}")
                    .ToString();

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(payload);
                    log.LogInformation(payload);
                }
                request.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            }

            request.Headers["Metadata"] = "true";
            

            log.LogInformation("We are preparing to get the access token");
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                string stringResponse = streamResponse.ReadToEnd();

                if ((int)response.StatusCode >= 400)
                {
                    log.LogError(stringResponse);
                    throw new Exception();
                }

                log.LogInformation(stringResponse);

                Dictionary<string, string> oauthResults = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringResponse);
                accessToken = oauthResults["access_token"];
            }
            catch (Exception e)
            {
                string errorText = String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Acquire token failed");
                log.LogError(errorText);
                throw new Exception();
            }
        }
    }
}