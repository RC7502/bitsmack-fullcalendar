using System;
using System.Collections.Generic;
using System.Configuration;
using Ical.Net;
using System.IO;
using System.Net;
using Ical.Net.Interfaces;

namespace BSFullCalendar.Models
{
    internal class FBEventModel
    {
        public FBEventModel()
        {

        }

        public IEnumerable<FCEventModel> Events()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "fbevents.ics");
            var file = new FileInfo(filePath);
            IICalendarCollection calList = new CalendarCollection();

            var list = new List<FCEventModel>();
            var fbEventFeed = ConfigurationManager.AppSettings["FBEventFeed"];
            var fbEventKey = ConfigurationManager.AppSettings["FBEventKey"];
            var fullPath = fbEventFeed + "&key=" + fbEventKey;

            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
           if(!IsFileLocked(file) && file.LastWriteTime < DateTime.Now.AddMinutes(-30))
                client.DownloadFile(fullPath, filePath);
            if(!IsFileLocked(file))
                calList = Calendar.LoadFromFile(filePath);

            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            foreach (var cal in calList)
            {
                foreach (var item in cal.Events)
                {
                    var newEvent = new FCEventModel()
                    {
                        title = item.Summary,
                        start = TimeZoneInfo.ConvertTimeFromUtc(item.Start.Value, easternZone).ToString(),
                        end = TimeZoneInfo.ConvertTimeFromUtc(item.End.Value, easternZone).ToString()
                    };
                    list.Add(newEvent);
                }
            }

            return list;

        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}