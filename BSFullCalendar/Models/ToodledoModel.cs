﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Web;
using Toodledo.Client;
using Toodledo.Model;
using Toodledo.Model.API;

namespace BSFullCalendar.Models
{
    public class ToodledoModel
    {
        private Session _session = null;
        public Session Session
        {
            get
            {
                var userid = ConfigurationManager.AppSettings["ToodledoUser"];
                var pw = ConfigurationManager.AppSettings["ToodledoPW"];
                var clientID = ConfigurationManager.AppSettings["ToodledoClientID"];
                return _session ?? Session.Create(userid, pw, clientID);
            }
        }

        public IGeneral General
        {
            get { return (IGeneral)this.Session; }
        }

        public ITasks Tasks
        {
            get { return (ITasks)this.Session; }
        }

        public INotebook Notebook
        {
            get { return (INotebook)this.Session; }
        }

        public IEnumerable<FCEventModel> GetTasks()
        {
            var list = new List<FCEventModel>();
            var query = new TaskQuery()
            {
                NotCompleted = true
            };
            var taskResult = Tasks.GetTasks(query);
            foreach (var task in taskResult.Tasks.Where(x=>x.Due.Year!=1))
            {
                list.Add(Converter.ToEvent(task));
            }
            return list;
        }

        public FCEventModel UpdateTask(FCEventModel model)
        {
            Task task;
            if (model.id != 0)
            {
                var query = new TaskQuery()
                {
                    Id = model.id
                };
                task = Tasks.GetTasks(query).Tasks.FirstOrDefault();
            }
            else
            {
                task = new Task();
            }
            if (task != null)
            {
                    task.Name = model.title;
                    if (task.Completed.Year.Equals(1) && model.completed)
                        task.Completed = DateTime.Today;
                    if (!task.Completed.Year.Equals(1) && !model.completed)
                        task.Completed = new DateTime(1, 1, 1);
                    var startTime = DateTime.Parse(model.start, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    if (model.allDay)
                    {
                        task.Start = startTime.Date;
                        var endTime = model.end != null
                            ? DateTime.Parse(model.end, null, System.Globalization.DateTimeStyles.RoundtripKind)
                            : startTime;
                        task.Due = endTime.Date.AddDays(-1);
                    }
                    else
                    {
                        task.Start = startTime;
                        var endTime = model.end != null
                            ? DateTime.Parse(model.end, null, System.Globalization.DateTimeStyles.RoundtripKind)
                            : startTime.AddMinutes(30);
                        task.Due = (endTime.TimeOfDay.TotalSeconds.Equals(0) ? endTime.AddDays(-1) : endTime);
                    }                                   
            }
            if (model.id == 0)
            {
                var newid = Tasks.AddTask(task);
                model = Converter.ToEvent(Tasks.GetTasks(new TaskQuery() {Id = newid}).Tasks.FirstOrDefault());
            }
            else
                Tasks.EditTask(task);

            return model;
        }
    }
}