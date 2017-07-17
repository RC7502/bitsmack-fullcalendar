var cal;

$('document').ready(function() {
    cal = $('#calendar');
});

function BuildCalendar() {
    GeneralSettings();
    GoogleCalendar();
    LoadTasks();
    LoadFBEvents();
}

function GeneralSettings() {
    cal.fullCalendar({
        nowIndicator: true,
        theme: true,
        defaultView: 'agendaWeek',
        eventDrop: function (event, delta, revertFunc) {
            EditEvent(event);
        },
        eventResize: function(event, delta, revertFunc) {
            EditEvent(event);
        },
        eventOrder: "priority",
        header:{
            right: 'month,agendaWeek,agendaDay'
        },
        forceEventDuration: true,
        defaultTimedEventDuration: '00:30:00',
        viewRender: function() {
            LoadWeather();
        },
        eventRender: function (event, element) {
            if (event.rendering === 'background') {
                element.append(event.title);
            }
            if (event.app === "Toodledo") {
                TaskRender(event, element);
            }
        },
        dayClick: function(date) {
            NewItemPopup(date);
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
            $("#calendar").fullCalendar('option','googleCalendarApiKey', gcal.APIKey);
            $("#calendar").fullCalendar('addEventSource',
            {
                googleCalendarId: gcal.CalID,
                color: 'green'
            });

        }
    });
}

function LoadTasks() {
    $.ajax({
        type: "POST",
        url: 'Home/GetTasks',
        dataType: 'json',
        contentType: "application/json; charset=utf-8",
        success: function(result) {
            $("#calendar").fullCalendar('removeEvents',
                function(event) {
                    return event.app === "Toodledo";
                });
            $("#calendar").fullCalendar('addEventSource',
            {
                color: 'yellow',
                events: result,
                textColor: 'black',
                editable: true
            });
        },
        error: function(jqXHR, textStatus, errorThrown) {
                window.alert(errorThrown);
        }
    });

}

function TaskRender(event, element) {
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

function LoadFBEvents() {
    $("#calendar").fullCalendar('addEventSource',
        {
            url: 'Home/GetFBEvents',
            color: 'blue'
        });
}

function LoadWeather() {
    var currentView = $("#calendar").fullCalendar("getView");
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
        success: function (newEvent) {
            if (newEvent.app === "Toodledo")
                LoadTasks();
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