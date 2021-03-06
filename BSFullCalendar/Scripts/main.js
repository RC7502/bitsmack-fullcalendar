﻿var cal;

$('document').ready(function() {
    cal = $('#calendar');
});

function BuildCalendar() {
    GeneralSettings();
    GoogleCalendar();
    LoadFBEvents();
}

function GeneralSettings() {
    cal.fullCalendar({    
        height: $(window).height()*0.93,
        nowIndicator: true,
        theme: true,
        defaultView: 'agendaWeek',
        scrollTime: '05:00:00',
        eventDrop: function (event, delta, revertFunc) {
            EditEvent(event);
        },
        eventResize: function(event, delta, revertFunc) {
            EditEvent(event);
        },
        eventOrder: "priority",
        header: {
            left: 'today,prev,next',
            center: 'title',
            right: 'month,agendaWeek,agendaDay'
        },
        forceEventDuration: true,
        defaultTimedEventDuration: '00:30:00',
        viewRender: function() {
            LoadWeather();
            LoadTasks();
        },
        eventRender: function (event, element) {
            $(element).tooltip({ title: event.title });
            if (event.rendering === 'background') {
                element.append(event.title);
            }
            if (event.app === "Toodledo") {
                TaskRender(event, element);
            }
        },
        dayClick: function(date) {
            NewItemPopup(date);
        },
        droppable: true,
        drop: function (date) {
            var eventData = $(this).data('event');
            if (date.hasTime())
            {
                eventData.allDay = false;
                eventData.start = date;
                eventData.end = moment(date).add(30, 'm');
            } else {
                eventData.allDay = false;
                eventData.start = date;
                eventData.end = date;
            }            
            EditEvent(eventData);
            $(this).remove();
        },
        eventDragStop: function (event, jsEvent) {
            var unSched = $("#taskList");
            var ofs = unSched.offset();
            var x1 = ofs.left;
            var x2 = ofs.left + unSched.outerWidth(true);
            var y1 = ofs.top;
            var y2 = ofs.top + unSched.outerHeight(true);

            if (jsEvent.pageX >= x1 && jsEvent.pageX <= x2 &&
                jsEvent.pageY >= y1 && jsEvent.pageY <= y2) {
                cal.fullCalendar('removeEvents', event.id);
                event.start = '';
                event.end = '';
                EditEvent(event);
            }
            
        }
    });
}

function GoogleCalendar() {
    var gcal;
    $.ajax({
        type: "GET",
        url: "Home/GCal",
        success: function (result) {
            gcal = result;
            cal.fullCalendar('option','googleCalendarApiKey', gcal.APIKey);
            cal.fullCalendar('addEventSource',
            {
                googleCalendarId: gcal.CalID,
                color: 'green'
            });

        }
    });
}

function LoadTasks() {
    var currentView = cal.fullCalendar("getView");
    var viewModel = {
        name: currentView.name
    }
    $.ajax({
        type: "POST",
        url: 'Home/GetTasks',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        data: '{view: ' + JSON.stringify(viewModel) + '}',
        success: function (result) {
            if (result.message !== "0") {
                window.alert(result.message);
            } else {
                cal.fullCalendar('removeEvents',
                    function(event) {
                        return event.app === "Toodledo";
                    });
                cal.fullCalendar('addEventSource',
                {
                    color: 'yellow',
                    events: result.list1,
                    textColor: 'black',
                    editable: true
                    });
                BuildTaskList(result.list2);
            }
        },
        error: function(jqXHR, textStatus, errorThrown) {
                window.alert(errorThrown);
        }
    });

}

function TaskRender(event, element) {
    if (!event.repeated) {
        var checkbox = $("<input type='checkbox'></input>");
        checkbox.change(function() {
            if (this.checked) {
                event.completed = true;
                cal.fullCalendar("removeEvent", event);
            } else {
                event.completed = false;
            }
            EditEvent(event);
        });
        element.find(".fc-title").prepend(checkbox);
    }
}

function LoadFBEvents() {
    $("#calendar").fullCalendar('addEventSource',
        {
            url: 'Home/GetFBEvents',
            color: 'blue'
        });
}

function LoadWeather() {
    var currentView = cal.fullCalendar("getView");
    var viewModel = {
        name: currentView.name
    }
    $.ajax({
        type: "POST",
        url: 'Home/GetWeather',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        data: '{model: ' + JSON.stringify(viewModel) + '}',
        success: function (result) {
            $("#calendar").fullCalendar('removeEvents', function (event) {
                return event.app === "Weather";
            });
            $("#calendar").fullCalendar('addEventSource',{

                id: "weather",
                events: result,
                color: '#ddf1e6',
                textColor: 'black'
            });
        }
    });

}

function EditEvent(event) {
    event.source = null;
    $.ajax({
        type: "POST",
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        url: "Home/EditEvent",
        data: '{model: ' + JSON.stringify(event) + '}',
        success: function(newEvent) {
            if (newEvent.app === "Toodledo") {
                LoadTasks();
            }
        }
    });

}

function NewItemPopup(date) {
    var dialog = $("#newItemDialog").dialog();    
    $("#itemDate").val(date);
    $("#itemTitle").val("");
    dialog.find("form").on("submit", function (event) {
        event.preventDefault();
        var allDay = function() {
            return !date.hasTime();
        }
        var newEvent = {
            allDay: allDay,
            start: date,
            title: $("#itemTitle").val(),
            app: $("#itemApp").val()
        }
        EditEvent(newEvent);
        dialog.dialog("destroy");
        dialog.remove();
    });
    dialog.dialog("open");

}

function BuildTaskList(list) {
    ResetTaskListHtml();
    list.forEach(function(task) {
        var newDiv = $('<div style="margin-top:10px"></div>');
        newDiv.addClass('fc-event');
        newDiv.css({
            'background-color': task.color,
            'color': 'black'
        });
        newDiv.draggable({
            zIndex: 999,
            revert: true,      // will cause the event to go back to its
            revertDuration: 0  //  original position after the drag
        });
        newDiv.data('event', task);
        var titleDiv = $("<div></div>");
        titleDiv.addClass('fc-title').html(task.title);
        newDiv.append(titleDiv);
        TaskRender(task, newDiv);
        switch (task.priority) {
            case 3:
                $("#top").append(newDiv);
                break;
            case 2:
                $("#high").append(newDiv);
                break;
            case 1:
                $("#med").append(newDiv);
                break;
            case 0:
                $("#low").append(newDiv);
                break;
            case -1:
                $("#neg").append(newDiv);
                break;
        }
        
    });
}

function ResetTaskListHtml() {
    $("#taskList").html("");
    $("#taskList").append("<div id='top'>Top</div><hr/>");
    $("#taskList").append("<div id='high'>High</div><hr/>");
    $("#taskList").append("<div id='med'>Medium</div><hr/>");
    $("#taskList").append("<div id='low'>Low</div><hr/>");
    $("#taskList").append("<div id='neg'>Negative</div><hr/>");
}