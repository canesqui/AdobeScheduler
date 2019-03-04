/* Adobe Calendar core file
 * Modified: 5/30/2018
 * Bernardo Martinez
 * Change: Update/Upgrade polling subroutines to decresease the load time
 * 
/*
* PLEASE NOTE, event = appointment model { they are synonymous }
*/

String.prototype.formatUnicorn = String.prototype.formatUnicorn ||
    function () {
        "use strict";
        var str = this.toString();
        if (arguments.length) {
            var t = typeof arguments[0];
            var key;
            var args = ("string" === t || "number" === t) ?
                Array.prototype.slice.call(arguments)
                : arguments[0];

            for (key in args) {
                str = str.replace(new RegExp("\\{" + key + "\\}", "gi"), args[key]);
            }
        }

        return str;
    };

$(function () {
    //Append loading text
    $('#calendar').html("<div id='calendarLoad'><h1>loading. . .</h1></div>");

    window.alert = function (message) {
        new PNotify({
            title: 'Alert',
            text: message,
            type: 'info',
            history: {
                menu: true
            }
        });

    };

    var adobeConnect = $.connection.adobeConnect;

    $('#addAppointment').dialog({
        autoOpen: false,
        show: {
            effect: "fade",
            duration: 300
        },
        hide: {
            effect: "fade",
            duration: 300
        },
        height: 550,
        width: 675,
        modal: true
    });

    $('body').on("change", "#class", function () {
        let e = document.getElementById("class");
        let opt = e.options[e.selectedIndex].getAttribute("data-url");
        let linkText = e.value + " Room Link";
        $('#room_link').text(linkText);
        document.getElementById("room_link").href = opt;
    });


    function getDataFromUI() {

        let eventId = $('#eventId').val() === "" ? "0" : $('#eventId').val();
        let selected = document.getElementById("class");
        let url = selected.options[selected.selectedIndex].getAttribute("data-url");
        let path = selected.options[selected.selectedIndex].getAttribute("data-path");
        let userId = $('#content').attr('data-userId');
        let class_name = $('#class option:selected').text();
        let datetime = $('#datetime').val();
        let room_size = $('#occupants').val();
        let end = $('#duration option:selected').attr('value');
        let repetitionId = $('#repetitionId').val();

        let repType = $('#repetition option:selected').text();

        datetime = moment(datetime).utc();

        let endEvent = moment(datetime, 'MM/DD/YYYY hh:mm A').add(end, 'minutes').format("MM/DD/YYYY hh:mm A");

        let endRepDate = $('#repetition_date').val() === "" ? endEvent : $('#repetition_date').val();

        endRepDate = moment(endRepDate).utc();

        let object = {
            id: eventId,
            userId: userId,
            title: class_name,
            url: path,
            adobeUrl: url,
            start: datetime,
            end: endEvent,
            allDay: false,
            roomSize: room_size,
            repetitionId: repetitionId,
            endRepDate: endRepDate,
            repetitionType: repType
        };
        return object;
    }

    function genericSaveOrUpdateCallback(result, rollbackAction) {
        if (result) {
            notifier(false, "Save", "The event was saved.", null, null, 'success');
        } else {
            if (rollbackAction !== null) {
                rollbackAction();
            }
            notifier(false, "Error", "An error occurred during event creation or update.", null, null, 'error');
        }
    }

    async function SaveOrUpdate(event) {
        //This will clean up additional data send by "fullcalendar" component
        var object = {
            id: event.id,
            userId: event.userId,
            title: event.title,
            url: event.url,
            adobeUrl: event.adobeUrl,
            start: event.start,
            end: event.end,
            allDay: false,
            roomSize: event.roomSize,
            isEditable: event.editable,
            repetitionId: event.repetitionId,
            endRepDate: event.endRepDate,
            repetitionType: event.repetitionType
        };

        return await adobeConnect.server.saveOrUpdate(object);
    }

    function getDuration(start, end) {
        start = new Date(start);
        end = new Date(end);
        return parseInt(end - start) / (1000 * 60);
    }

    function confirmation(question) {
        var defer = $.Deferred();
        $('<div></div>')
            .html(question)
            .dialog({
                autoOpen: true,
                modal: true,
                title: 'Confirmation',
                buttons: {
                    "Yes": function () {
                        defer.resolve("true");//this text 'true' can be anything. But for this usage, it should be true or false.
                        $(this).dialog("close");
                    },
                    "No": function () {
                        defer.resolve("false");//this text 'false' can be anything. But for this usage, it should be true or false.
                        $(this).dialog("close");
                    }
                }
            });
        return defer.promise();
    }

    adobeConnect.client.RemoveEvent = function (id) {
        $('#calendar').fullCalendar('removeEvents', id);
    };

    adobeConnect.client.UpdateEvent = function (event) {
        $('#calendar').fullCalendar('removeEvents', event.id);
        $('#calendar').fullCalendar('renderEvent', event, true);
    };

    function notifier(pd, title, message, cb, data, type) {
        if (pd) {
            var cur_value = 1,
                progress;
            var loader = new PNotify({
                title: title,
                text: "<div class=\"progress_bar\" />",
                icon: 'picon picon-throbber',
                hide: false,
                closer: false,
                sticker: false,
                history: false,
                type: type,
                before_open: function (pnotify) {
                    progress = pnotify.find("div.progress_bar");
                    progress.progressbar({
                        value: cur_value
                    });
                    var timer = setInterval(function () {
                        if (cur_value >= 100) {
                            window.clearInterval(timer);
                            loader.pnotify_remove();
                            cb(data);
                            return;
                        }
                        cur_value += .3;
                        progress.progressbar('option', 'value', cur_value);
                    }, 2);
                }
            });
        } else {
            if (cb) {
                cb();
            }
            new PNotify({
                title: title,
                text: message,
                closer: false,
                sticker: false,
                type: type,
                delay: 7000,
                before_close: function (pnotify) {
                    return true;
                }
            });
        }
    }

    Calendar = function (events) {
        //remove loading text, init adobecal
        $('#calendarLoad').remove();
        $('#calendar').fullCalendar({
            header: {
                left: 'prev,next today month',
                right: ''
            },
            views: {
                month: {
                    titleFormat: 'YYYY MMMM'
                },
                agendaDay: {
                    titleFormat: 'ddd MM/DD/YYYY'
                }
            },
            timezone: 'local',
            monthNames: [
                'JANUARY',
                'FEBRUARY',
                'MARCH',
                'APRIL',
                'MAY',
                'JUNE',
                'JULY',
                'AUGUST',
                'SEPTEMBER',
                'OCTOBER',
                'NOVEMBER',
                'DECEMBER'
            ],
            monthNamesShort: [
                'JAN',
                'FEB',
                'MAR',
                'APR',
                'MAY',
                'JUN',
                'JUL',
                'AUG',
                'SEP',
                'OCT',
                'NOV',
                'DEC'
            ],
            dayNames: [
                'SUN',
                'MON',
                'TUE',
                'WED',
                'THU',
                'FRI',
                'SAT'],
            buttonText: {
                prev: "",
                next: "",
                prevYear: "",
                nextYear: "",
                today: 'TODAY',
                month: 'MONTH'
            },
            defaultView: 'month',
            editable: false,
            eventAfterRender: function (event, element, view) {
                var height = $(element).height();
            },
            loading: function (isLoading) {
                if (!isLoading) {
                    $('#busy1');
                }
            },
            eventRender: function (event, element, view) {
                var roomHtml;

                if (view.name === 'agendaDay') {
                    roomHtml = event.title + "  " + event.roomSize;
                    element.find(".fc-title")
                        .html(roomHtml);
                }

                if (view.name === 'month') {
                    roomHtml = "</br><b>Occupants</b>: " + "<u>" + event.roomSize + "</u>";
                    element.find(".fc-content")
                        .append(roomHtml);
                }

                if (event.editable && !event.archived) {                    
                    var html = '<a id="editEvent" href="#' + event.id + '"><i class="ui-icon ui-icon-pencil" style="float:right;"></i></a>';
                    element.find(".fc-title").append(html);
                }
            },
            viewRender: function (view) {
                let title = view.title;
                title = title.split(" ");
                let htmlText = '';
                if (view.name === 'month') {
                    htmlText = "<span class='year'>{0}</span><span class='month'>{1}</span>".formatUnicorn(title[0], title[1]);
                }
                else {
                    htmlText = '<span class="day">{0}</span><span class="date">{1}</span>'.formatUnicorn(title[0].toUpperCase(), title[1]);
                }
                $("#fctitle").html(htmlText);
            },
            events: function (start, end, timezone, cb) {
                cb(events);
            },
            dayClick: function (date, jsEvent, view) {                
                if (view.name === 'month') {
                    $('#calendar').fullCalendar('changeView', 'agendaDay');
                    $('#calendar').fullCalendar('gotoDate', date);
                }

                if (view.name === 'agendaDay') {
                    $('#datetime').val(moment(date).format("MM/DD/YYYY hh:mm:ss A "));                    
                    if (moment(moment().utc().local().subtract(30, 'minutes')).isAfter(moment(date, "MM/DD/YYYY hh:mm:ss A "))) {
                        alert("Events cannot be created in the past.");
                        return;
                    }
                    $('#addAppointment').dialog({   //
                        title: "Create Appointment",
                        buttons:
                            [
                                {
                                    id: 'createAppointment',
                                    text: 'Create Appointment',
                                    //Click funtion: Gets the current time from the calendar and warns the user if there's a meeting right before it.       
                                    click: function () {
                                        let events = $('#calendar').fullCalendar('clientEvents');
                                        let checkPrevious = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a').subtract(1, "minutes");
                                        let checkFollowing = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a').add(60, "minutes");
                                        events.forEach(function (event) {
                                            let eventtimeend = event.end;
                                            let eventtimestart = event.start;
                                            if (checkPrevious.isSame(eventtimeend)) {
                                                alert("Warning! There's a meeting finishing right before you.");
                                            }
                                            if (checkFollowing.isSame(eventtimestart)) {
                                                alert("Warning! There's a meeting right after you.");
                                            }
                                        });
                                        $('#eventId').val(0);
                                        let event = getDataFromUI();
                                        SaveOrUpdate(event).then(function (result) {
                                            genericSaveOrUpdateCallback(result, null);
                                        });
                                        $(this).dialog("close");
                                    }
                                },
                                {
                                    text: 'Cancel',
                                    click: function () { $(this).dialog("close") }
                                }

                            ]
                    });
                    callback = function (e) {
                        $('#occupants').val(e);
                        $('#addAppointment').dialog('open');
                        updateUI(e);
                    };
                    checkAvailableLicenses(moment(date).format("MM/DD/YYYY hh:mm A "), moment(date).add($('#duration').val(), 'minutes').format("MM/DD/YYYY hh:mm A")).then(function (e) { callback(e); });
                }
            },
            eventClick: function (event, element) {
                if (element.target.tagName === 'I') {
                    $('#eventId').val(event.id);
                    $('#class option:selected').text(event.title);
                    $('#datetime').val(moment(event.start).format("MM/DD/YYYY hh:mm A "));
                    if (event.endRepDate !== null) {
                        $('#repetition_date').val(moment(event.endRepDate).format("MM/DD/YYYY hh:mm A "));
                    } else {
                        $('#repetition_date').val(getDuration(event.start, event.end));
                    }
                    $('#duration').val(getDuration(event.start, event.end));
                    $('#repetitionId').val(event.repetitionId);
                    $('#repetition').val(event.repetitionType);
                    $(".chosen-select").trigger("chosen:updated");
                    $('#addAppointment').dialog({
                        title: "Update/Delete Appointment",
                        buttons: {
                            "delete": {
                                text: 'Delete',
                                class: 'delete',
                                click: function () { Delete(getDataFromUI()); }
                            },
                            "update": {
                                id: 'create',
                                text: 'Update',
                                class: 'update',
                                click: function () { Update(getDataFromUI()); }
                            },
                            "cancel": {
                                text: 'Cancel',
                                click: function () { $(this).dialog('close'); }
                            }
                        }
                    });

                    callback = function (e) {
                        $('#occupants').val(event.roomSize);
                        updateUI(e);
                        $('#addAppointment').dialog('open');
                    };

                    checkAvailableLicenses(event.start, event.end, { eventId: event.id, repetitionType: event.repetitionType }).then(function (e) { callback(e); });

                    GenericSingleEventDeleteCallback = function (result) {
                        if (result) {
                            notifier(false, "Delete", "The event was deleted.", null, null, 'success');
                        } else {
                            notifier(false, "Error", "An error occurred during event deletion.", null, null, 'error');
                        }
                    };

                    GenericSeriesEventDeleteCallback = function (result) {
                        if (result) {
                            notifier(false, "Delete", "The selected event and all other events in the series has been deleted.", null, null, 'success');
                        } else {
                            notifier(false, "Error", "An error occurred during event series deletion.", null, null, 'error');
                        }
                    };

                    Delete = function (appointment) {

                        confirmation('Are you sure you want to permanantly delete this appoinment?').then(function (answer) {
                            if (answer === "false") {
                                notifier(false, "Canceled", "Transaction Canceled.", null, null, 'success');
                            }
                            else {
                                if (event.repetitionType === "None") {
                                    adobeConnect.server.delete(appointment).then(function (e) { GenericSingleEventDeleteCallback(e); });
                                }
                                else {
                                    confirmation('The selected event is a repeating event, would you like to remove them all?').then(function (answer) {
                                        if (answer === "false") {
                                            appointment.repetitionType = "None";
                                            adobeConnect.server.delete(appointment).then(function (e) { GenericSingleEventDeleteCallback(e); });
                                        } else {
                                            adobeConnect.server.delete(appointment).then(function (e) { GenericSeriesEventDeleteCallback(e); });
                                        }
                                    });
                                }
                                $('#addAppointment').dialog('close');
                            }
                        });
                    };
                    Update = function (event) {
                        if (event.repetitionType !== "None") {
                            confirmation('The selected event is a repeating event, would you like to modify the entire series?').then(function (answer) {
                                if (answer === "false") {
                                    event.repetitionType = "None";
                                }
                                SaveOrUpdate(event).then(function (result) {
                                    genericSaveOrUpdateCallback(result, null);
                                });
                            });
                        } else {
                            SaveOrUpdate(event).then(function (result) {
                                genericSaveOrUpdateCallback(result, null);
                            });
                        }
                        $('#addAppointment').dialog('close');
                    };
                }
                else {
                    if (event.adobeUrl) {
                        window.open(event.adobeUrl, event.title);
                    }
                }
                return false;
            },
            eventDrop: function (event, delta, revertFunc, jsEvent, ui, view) {
                if (moment(moment().utc().local().subtract(30, 'minutes')).isAfter(moment(event.start, 'MM/DD/YYYY hh:mm:ss A '))) {
                    alert("Events cannot be created in the past.");
                    revertFunc();
                    return false;
                }

                let callbackEventDrop = function (result) {
                    if (event.roomSize > result) {
                        revertFunc();
                        alert("There are not enough licenses available.");
                        return false;
                    }
                    if (event.repetitionType !== "None") {
                        confirmation('The selected event is a repeating event, would you like to change the duration to all of them?').then(function (answer) {
                            if (answer === "false") {
                                event.repetitionId = null;
                                event.repetitionType = "None";
                            }
                        });
                    }
                    SaveOrUpdate(event).then(function (result) {
                        genericSaveOrUpdateCallback(result, revertFunc);
                    });
                    return true;
                };
                checkAvailableLicenses(event.start, event.end, { eventId: event.id, repetitionType: event.repetitionType, endRepetition: event.endRepDate, repetitionId: event.repetitionId }).then(function (e) { callbackEventDrop(e); });

            },
            eventDragStart: function (event, jsEvent, ui, view) {
            },
            eventDragStop: function (event, jsEvent, ui, view) {
            },
            eventResize: function (event, delta, revertFunc, jsEvent, ui, view) {
                if (getDuration(event.start, event.end) > 90) {
                    revertFunc();
                    alert("Events Cannot be longer than 90 minutes.");
                    return false;
                }
                if (moment(moment().utc().local().subtract(30, 'minutes')).isAfter(moment(event.start, 'MM/DD/YYYY hh:mm:ss A '))) {
                    alert("Events cannot be moved to the past");
                    revertFunc();
                    return false;
                }

                let callbackEventResize = function (e) {
                    if (event.roomSize > e) {
                        revertFunc();
                        alert("There are not enough licenses available.");
                        return false;
                    }

                    if (event.repetitionType !== "None") {
                        confirmation('The selected event is a repeating event, would you like to change the duration to all of them?').then(function (answer) {
                            if (answer === "false") {
                                event.repetitionId = null;
                                event.repetitionType = "None";
                                SaveOrUpdate(event);
                            }
                            else {
                                SaveOrUpdate(event);
                            }
                            notifier(false, "Updating", "Event: " + event.title, null, null, 'info');
                            return true;
                        });
                    } else {
                        SaveOrUpdate(event).then(function (result) {
                            genericSaveOrUpdateCallback(result, revertFunc);
                        });
                    }

                };
                checkAvailableLicenses(event.start, event.end, { eventId: event.id, repetitionType: event.repetitionType, endRepetition: event.endRepDate, repetitionId: event.repetitionId }).then(function (e) { callbackEventResize(e); });
            }
        });
    };

    $.connection.hub.start().done(function () {
        adobeConnect.server.getAllAppointments(moment().format("MM/DD/YYYY hh:mm A")).done(Calendar);
        adobeConnect.server.getRooms().done(function (result) {
            if (result.length !== null) {       
                console.log(result);
                $('#class').find('option').remove().append('<option selected="selected" disabled="disabled" data-path="" data-url="">Select Your Room:</option>');
                for (var i = 0; i < result.length; i++) {
                    $('#class').append("<option data-path=\"" + result[i].url + "\" data-url=\"" + result[i].adobeUrl + "\">" + result[i].meetingName + "</option>");
                }
                $(".chosen-select").trigger("chosen:updated");               
            }
        });
    });

    $('.numbersOnly').keyup(function () {
        this.value = this.value.replace(/[^0-9\.]/g, '');
    });

    function updateUI(value) {
        $('#availableLicenses').text(value);
        if ($('#occupants').val() > value || value === 0) {
            $('#createAppointment').attr("disabled", true).css('opacity', 0.5);
            $('#create').attr("disabled", true).css('opacity', 0.5);
        } else {
            $('#createAppointment').attr("disabled", false).css('opacity', 1);
            $('#create').attr("disabled", false).css('opacity', 1);
        }
    }

    $('#occupants').keyup(function () {
        inputDataChanged();
    });

    function inputDataChanged() {

        let start = moment($("#datetime").val(), 'MM/DD/YYYY hh:mm A').format("MM/DD/YYYY hh:mm A");
        let end = moment($("#datetime").val(), 'MM/DD/YYYY hh:mm A').add($("#duration").val(), 'minutes').format("MM/DD/YYYY hh:mm A");
        let repId = $("#repetitionId").val();
        let endRepetition = moment($('#repetition_date').val(), 'MM/DD/YYYY hh:mm A').format("MM/DD/YYYY hh:mm A");
        let repetitionType = $('#repetition option:selected').text();
        let eventId = $("#eventId").val();
        let callback = function (e) {
            updateUI(e);
        };
        checkAvailableLicenses(start, end, { eventId: eventId, repetitionType: repetitionType, endRepetition: endRepetition }).then(function (e) { callback(e); });
    }

    $('#duration').on('change', function () {
        inputDataChanged();
    });

    // DateTimePicker Set Up
    $('#datetime').datetimepicker({
        minDate: 0,
        timeFormat: "hh:mm TT",
        minuteGrid: 10
    });

    $('#datetime').on('hide', function () {
        inputDataChanged();
    });

    async function checkAvailableLicenses(startingDateTime, endingDateTime, opts) {

        startingDateTime = moment(startingDateTime).utc();

        endingDateTime = moment(endingDateTime).utc();

        let eventId = opts !== undefined ? opts['eventId'] : null;

        let repetitionId = opts !== undefined ? opts['repetitionId'] : null;

        let repetitionType;

        if (opts !== undefined) {
            repetitionType = opts['repetitionType'] !== undefined ? opts['repetitionType'] : "None";
        } else {
            repetitionType = "None";
        }

        let endRepetition = opts !== undefined ? opts['endRepetition'] : endingDateTime;

        if (endRepetition !== "Invalid date") {
            endRepetition = moment(endRepetition).utc();
            endRepetition = moment(endRepetition).format("MM/DD/YYYY hh:mm A");
        } else {
            endRepetition = null;
        }

        if (repetitionType !== "None" && endRepetition !== null) {
            return await adobeConnect.server.checkAvailableLicenses(startingDateTime, endingDateTime, repetitionType, endRepetition, repetitionId);
        }
        else {
            return await adobeConnect.server.checkAvailableLicenses(startingDateTime, endingDateTime, eventId);
        }
    }

    $('#repetition_date').on('change', function () {
        inputDataChanged();
    });

    $('#repetition').on('change', function () {
        inputDataChanged();
    });

    $('#repetition_date').datetimepicker({
        minDate: 0,
        timeFormat: "hh:mm TT",
        minuteGrid: 10
    });

    $('body').on('click', '.ui-datepicker-close', function () {
        inputDataChanged();
    });
});