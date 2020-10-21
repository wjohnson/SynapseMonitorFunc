# Timer Query of Azure Monitor's Alerts

A timer based trigger to look at Log Analytics active alerts.

## Roles

* An Azure Function should have managed identity
* The Az Func managed identity should have the Log Analytics Reader role on the Log Analytics Workspace that contains the alerts.

## Settings

```
{
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "sqlQuery": "SOME QUERY TO EXECUTE",
    "sqlDB": "Server=tcp:YOURDATABSE.database.windows.net,1433;Initial Catalog=SOMEDB;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;",
    "LOG_ANALYTICS_WORKSPACE_NAME":"YOUR_WORKSPACE_NAME",
    "RESOURCE_GROUP":"LOG_ANALYTICS_RESOURCE_GROUP_NAME",
    "SUBSCRIPTION_ID":"LOG_ANALYTICS_SUBSCRIPTION_ID",
    "ALERT_RULE_ID":"/subscriptions/{SUBSCRIPTION_ID}/resourceGroups/{RESOURCE_GROUP}/providers/microsoft.insights/scheduledqueryrules/{ALERT_RULE_NAME}"
  }
}

```