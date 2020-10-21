using System;
using System.Collections.Generic;


namespace com.sample
{
    public class Essentials    {
        public string severity { get; set; } 
        public string signalType { get; set; } 
        public string alertState { get; set; } 
        public string monitorCondition { get; set; } 
        public string monitorService { get; set; } 
        public string targetResource { get; set; } 
        public string targetResourceName { get; set; } 
        public string targetResourceGroup { get; set; } 
        public string targetResourceType { get; set; } 
        public string sourceCreatedId { get; set; } 
        public string smartGroupId { get; set; } 
        public string smartGroupingReason { get; set; } 
        public string alertRule { get; set; } 
        public DateTime startDateTime { get; set; } 
        public DateTime lastModifiedDateTime { get; set; } 
        public string lastModifiedUserName { get; set; } 
        public DateTime monitorConditionResolvedDateTime { get; set; } 
    }

    public class Properties    {
        public Essentials essentials { get; set; } 
    }

    public class Value    {
        public Properties properties { get; set; } 
        public string id { get; set; } 
        public string type { get; set; } 
        public string name { get; set; } 
    }

    public class AlertsResponse    {
        public string nextLink { get; set; } 
        public List<Value> value { get; set; } 
    }

}