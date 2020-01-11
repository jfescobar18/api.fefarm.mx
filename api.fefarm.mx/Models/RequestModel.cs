using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.fefarm.mx.Models
{
    public class RequestModel
    {
        public int id { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public Values values { get; set; }
        public Points points { get; set; }
        public List<string> answers { get; set; }
        public bool required { get; set; }
        public string size { get; set; }
    }
}