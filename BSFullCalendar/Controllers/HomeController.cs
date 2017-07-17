﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BSFullCalendar.Models;
using System.Configuration;
using Toodledo.Client;

namespace BSFullCalendar.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ToodledoModel todoModel; 

        public HomeController()
        {
            todoModel = new ToodledoModel();
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetTasks()
        {
            FCEventModel[] eventList;
            try
            {
                eventList = todoModel.GetTasks().ToArray();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(100,ex.Message);
            }
            return Json(eventList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFBEvents()
        {
            var model = new FBEventModel();
            var eventList = model.Events().ToArray();
            return Json(eventList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GCal()
        {
            var gcal = new GoogleCalModel
            {
                APIKey = ConfigurationManager.AppSettings["GoogleAPI"],
                CalID = ConfigurationManager.AppSettings["GoogleCalID"]
            };
            return Json(gcal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditEvent(FCEventModel model)
        {
            var newModel = model;
            switch (model.app)
            {
                case "Toodledo":
                    newModel = todoModel.UpdateTask(model);
                    break;
            }
            return Json(newModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWeather(FCViewModel model)
        {
            var wModel = new WeatherModel();
            var eventList = model.name == "month" ? wModel.DailyForecast().ToArray() : wModel.HourlyForecast().ToArray();
            return Json(eventList, JsonRequestBehavior.AllowGet);
        }

    }

}
