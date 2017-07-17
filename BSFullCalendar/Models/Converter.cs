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
                completed = false,
                color = GetColor(task),
                editable = true
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

        private static string GetColor(Task task)
        {
            if (task.Folder != null)
            {
                switch (task.Folder.Name)
                {
                    case "Health":
                        return "#54C6C6";
                    case "Home":
                        return "#9B6412";
                    case "Family":
                        return "#9BF66E";
                    case "Creative":
                        return "orange";
                    case "Errands":
                        return "fuchsia";
                    case "Social\\Party":
                        return "pink";
                }
            }
            return "yellow";
        }

        public static Task ToTask(FCEventModel model, Task task)
        {
            task.Name = model.title;
            //if checking/unchecking completed     
            if (task.Completed.Year.Equals(1) && model.completed)
                task.Completed = DateTime.Today;
            if (!task.Completed.Year.Equals(1) && !model.completed)
                task.Completed = new DateTime(1, 1, 1);


            //start and end time
            DateTime startTime;
            DateTime endTime;
            if(DateTime.TryParse(model.start, null, System.Globalization.DateTimeStyles.RoundtripKind, out startTime))
            {
                task.Start = model.allDay ? startTime.Date : startTime;
            }
            else
            {
                task.Start = DateTime.MinValue;
            }
            if (DateTime.TryParse(model.end, null, System.Globalization.DateTimeStyles.RoundtripKind, out endTime))
            {
                task.Due = model.allDay ? endTime.Date.AddDays(-1) : endTime;
            }
            else
            {
                task.Due = task.Start.Year != 1 ? task.Start.AddMinutes(30) : DateTime.MinValue;
            }

            return task;
        }
    }
}