using System;
using System.Collections.Generic;
using System.Configuration;
using ForecastIO;
using Microsoft.Ajax.Utilities;


namespace BSFullCalendar.Models
{
    internal class WeatherModel
    {
        public string ForecastKey;

        public WeatherModel()
        {
            ForecastKey = ConfigurationManager.AppSettings["ForecastKey"];
        }

        public IEnumerable<FCEventModel> DailyForecast()
        {
            var list = new List<FCEventModel>();        
            var request = new ForecastIORequest(ForecastKey, (float)40.083451, (float)-83.104796, Unit.us);
            var response = request.Get();
            foreach (var day in response.daily.data)
            {
                var newEvent = new FCEventModel()
                {
                    title = String.Format("{0} - {1}", Math.Round(day.temperatureMax,0), day.icon),
                    allDay = true,
                    start = new DateTime(1970,1,1).AddSeconds(day.time).ToString("yyyy-MM-dd"),
                    app = "Weather",
                    rendering = "background"
                };
                list.Add(newEvent);
            }

            return list;
        }

        public IEnumerable<FCEventModel> HourlyForecast()
        {
            var list = new List<FCEventModel>();
            var request = new ForecastIORequest(ForecastKey, (float)40.083451, (float)-83.104796, Unit.us);
            var response = request.Get();
            foreach (var hour in response.hourly.data)
            {
                var wTime = new DateTime(1970, 1, 1).AddSeconds(hour.time);
                var newEvent = new FCEventModel()
                {
                    title = String.Format("{0} - {1}", Math.Round(hour.temperature, 0), hour.icon),
                    allDay = false,
                    start = wTime.ToString("yyyy-MM-dd HH:mm"),   
                    end = wTime.AddMinutes(30).ToString("yyyy-MM-dd HH:mm"),
                    app = "Weather",
                    rendering = "background"
                };
                list.Add(newEvent);
            }

            return list;
        }
    }
}