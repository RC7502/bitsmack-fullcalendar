using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Toodledo.Model;

namespace BSFullCalendar.Models
{
    public class Converter
    {
        public static FCEventModel ToEvent(Task task)
        {
            var newEvent = new FCEventModel()
            {
                id = task.Id,
                title = task.Name,
                app = "Toodledo",
                completed = false

            };
            if (task.Due.TimeOfDay.TotalSeconds.Equals(0))
            {
                newEvent.allDay = true;
                newEvent.start = (task.Start.Year == 1 ? task.Due.ToString("yyyy-MM-dd") : task.Start.ToString("yyyy-MM-dd"));
                newEvent.end = (task.Start == task.Due
                    ? task.Due.ToString("yyyy-MM-dd")
                    : task.Due.AddDays(1).ToString("yyyy-MM-dd"));
            }
            else
            {
                newEvent.allDay = false;
                newEvent.start = (task.Start.Year == 1 ? task.Due.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm") : task.Start.ToString("yyyy-MM-dd HH:mm"));
                newEvent.end = task.Due.ToString("yyyy-MM-dd HH:mm");
            }
            return newEvent;
        }
    }
}