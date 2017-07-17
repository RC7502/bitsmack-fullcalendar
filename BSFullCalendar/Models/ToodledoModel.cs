using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
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

        private IEnumerable<Folder> _folders { get; set; }

        public IEnumerable<Folder> Folders
        {
            get
            {
                if (_folders == null)
                {
                    _folders = General.GetFolders();
                }
                return _folders;
            }
        }

        public FCResponseModel GetTasks()
        {
            var response = new FCResponseModel();
            try
            {
                var query = new TaskQuery()
                {
                    NotCompleted = true
                };
                var taskResult = Tasks.GetTasks(query);
                foreach (var task in taskResult.Tasks.OrderByDescending(x=>x.Added))
                {
                    task.Folder = GetFolder(task.Folder.Id);
                    if (task.Due != DateTime.MinValue)
                    {
                        response.list1.Add(Converter.ToEvent(task));
                        if(task.Repeat != Frequency.Once)
                            response.list1.AddRange(GenerateRecurring(task));
                    }
                    else
                        response.list2.Add(Converter.ToEvent(task));
                }
                
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
            }
            return response;
        }

        private IEnumerable<FCEventModel> GenerateRecurring(Task task)
        {
            var newTask = task;
            var list = new List<FCEventModel>();
            for (var i = 0; i < 10; i++)
            {
                switch (newTask.Repeat)
                {
                    case Frequency.Daily:
                        newTask.Due = newTask.Due == DateTime.MinValue ? DateTime.MinValue : newTask.Due.AddDays(1);
                        newTask.Start = newTask.Start == DateTime.MinValue ? DateTime.MinValue : newTask.Start.AddDays(1);
                        break;
                    case Frequency.Biweekly:
                        newTask.Due = newTask.Due == DateTime.MinValue ? DateTime.MinValue : newTask.Due.AddDays(14);
                        newTask.Start = newTask.Start == DateTime.MinValue ? DateTime.MinValue : newTask.Start.AddDays(14);
                        break;
                    case Frequency.Monthly:
                        newTask.Due = newTask.Due == DateTime.MinValue ? DateTime.MinValue : newTask.Due.AddMonths(1);
                        newTask.Start = newTask.Start == DateTime.MinValue ? DateTime.MinValue : newTask.Start.AddMonths(1);
                        break;
                    case Frequency.Semiannually:
                        newTask.Due = newTask.Due == DateTime.MinValue ? DateTime.MinValue : newTask.Due.AddMonths(6);
                        newTask.Start = newTask.Start == DateTime.MinValue ? DateTime.MinValue : newTask.Start.AddMonths(6);
                        break;
                    case Frequency.Advanced:
                        newTask = AdvRepeat(newTask);
                        break;
                    default:
                        newTask = AdvRepeat(newTask);
                        break;
                }
                var newEvent = Converter.ToEvent(newTask);
                newEvent.editable = false;
                list.Add(newEvent);
            }
            return list;
        }

        private Task AdvRepeat(Task task)
        {
            var textRepeat = task.AdvancedRepeat.Split(' ');
            if (textRepeat.Any())
            {
                if (textRepeat[0] == "Every")
                {
                    short intervalOrDay;
                    if (Int16.TryParse(textRepeat[1], out intervalOrDay))
                    {
                        switch (textRepeat[2])
                        {
                            case "day":
                            case "days":
                                task.Due = task.Due == DateTime.MinValue ? DateTime.MinValue : task.Due.AddDays(intervalOrDay);
                                task.Start = task.Start == DateTime.MinValue ? DateTime.MinValue : task.Start.AddDays(intervalOrDay);
                                break;
                            case "week":
                            case "weeks":
                                task.Due = task.Due == DateTime.MinValue ? DateTime.MinValue : task.Due.AddDays(intervalOrDay * 7);
                                task.Start = task.Start == DateTime.MinValue ? DateTime.MinValue : task.Start.AddDays(intervalOrDay * 7);
                                break;
                            case "month":
                            case "months":
                                task.Due = task.Due == DateTime.MinValue ? DateTime.MinValue : task.Due.AddMonths(intervalOrDay);
                                task.Start = task.Start == DateTime.MinValue ? DateTime.MinValue : task.Start.AddMonths(intervalOrDay);
                                break;
                        }
                    }
                    else
                    {
                        //assuming once a week for now
                        task.Due = task.Due == DateTime.MinValue ? DateTime.MinValue : task.Due.AddDays(7);
                        task.Start = task.Start == DateTime.MinValue ? DateTime.MinValue : task.Start.AddDays(7);
                    }
                }
            }

            return task;
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
                task = Converter.ToTask(model, task);                             
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

        public IEnumerable<FCEventModel> GetUnscheduledTasks()
        {
            var list = new List<FCEventModel>();
            var query = new TaskQuery()
            {
                NotCompleted = true
            };
            var taskResult = Tasks.GetTasks(query);
            foreach (var task in taskResult.Tasks.Where(x => x.Due.Year == 1))
            {
                list.Add(Converter.ToEvent(task));
            }
            return list;
        }

        public Folder GetFolder(int id)
        {
            return Folders.FirstOrDefault(x => x.Id == id);
        }
    }
}