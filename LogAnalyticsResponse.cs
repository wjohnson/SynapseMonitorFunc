using System;
using System.Collections.Generic;

namespace com.sample
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Column    {
        public string name { get; set; } 
        public string type { get; set; } 
    }

    public class Table    {
        public string name { get; set; } 
        public List<Column> columns { get; set; } 
        public List<List<object>> rows { get; set; } 
    }

    public class LogAnalyticsResponse    {
        public List<Table> tables { get; set; } 
    }


}