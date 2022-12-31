using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorxLogisticsAPIs.Models
{
    public class Response
    {
        public bool success { get; set; }
        public JObject result { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }
}