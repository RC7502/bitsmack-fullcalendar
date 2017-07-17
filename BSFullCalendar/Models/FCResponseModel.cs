using System.Collections.Generic;

namespace BSFullCalendar.Models
{
    public class FCResponseModel
    {
        public FCResponseModel()
        {
            list1 = new List<FCEventModel>();
            list2 = new List<FCEventModel>();
            message = "0";
        }

        public List<FCEventModel> list2 { get; set; }

        public List<FCEventModel> list1 { get; set; }
        public string message { get; set; }
    }
}