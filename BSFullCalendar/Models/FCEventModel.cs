using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BSFullCalendar.Models
{
    public class FCEventModel
    {
        public string title { get; set; }
        public bool allDay { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string app { get; set; }
        public int id { get; set; }
        public string priority { get; set; }
        public string rendering { get; set; }
        public bool completed { get; set; }
    }
}