﻿/* Adobe Calendar core file
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
    Events = {};
    
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

    IsUpdate = false;
    roomList = "";
    window.max = 70;
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
        modal: true,
        open: function () {
            addAppointment(false, IsUpdate);
        }
    });

    $('body').on("change", "#class", function () {
        var e = document.getElementById("class");
        var opt = e.options[e.selectedIndex].getAttribute("data-url");
        var linkText = e.value + " Room Link";
        $('#room_link').text(linkText);
        document.getElementById("room_link").href = opt;
    });

    function newTab(url) {
        var win = window.open(url, '_blank');
        win.focus();
    }

    addAppointment = function (checked, isUpdate, jsHandle, event, changeAll) {
        var roomId = window.Id;
        isUpdate = IsUpdate;
        var userId = (event == undefined) ? $('#content').attr('data-userId') : event.userId;
        var class_name = (event == undefined) ? $('#class option:selected').text() : event.title;
        var url = (event == undefined) ? $('#class option:selected').attr('data-url') : event.adobeUrl;
        var path = (event == undefined) ? $('#class option:selected').attr('data-path') : event.url;
        var datetime = (event == undefined) ? $('#datetime').val() : moment(event.start).format("MM/DD/YYYY hh:mm A");
        var room_size = (event == undefined) ? $('#occupants').val() : event.roomSize;
        var end = (event == undefined) ? $('#duration option:selected').attr('value') : getDuration(event.start, event.end);
        var js = (jsHandle == undefined) ? false : true;

        //if the selected DOM object is none, send false, otherwise true
        var isMultiple = ($('#repitition option:selected').text() === "None" ? false : true);//(event == undefined) ? ($('#repitition option:selected').text() === "none" ? false : true) : event.isMult;

        //if the event is undefined, check the DOM for the value, otherwise use the event value the event value will always be defined for rep items
        var repId = (event == undefined) ? ((isMultiple === false) ? null : String(moment().format("MM/DD/YYYY hh:mm A") + userId)) : event.repititionId;

        //if the event is undefined, check the DOM for the value, otherwise use the event value the event value will always be defined for rep items
        var JSendRepDate = (event == undefined) ? ((isMultiple === false) ? $('#datetime').val() : $('#repitition_date').val()) : moment(event.endRepDate).format("MM/DD/YYYY hh:mm A");

        //if the event is undefined, check the event value
        var repType = (event == undefined) ? $('#repitition option:selected').text() : event.repititionType;

        //if changeAll = undefined, assume false
        if (changeAll === undefined) {
            changeAll = false;
        }


        //console.log("This is is Multiple " + isMultiple);
        // console.log("RepId " + repId);
        adobeConnect.server.addAppointment(checked, isUpdate, roomId, userId, class_name, room_size, url, path, datetime, end, js, isMultiple, repId, JSendRepDate, repType, changeAll)
            .done(function (e) {
                return e;
            });

        /* !!ORIGIONAL CODE!! -- DO NOT DELETE
         * adobeConnect.server.addAppointment(checked, isUpdate, roomId, userId, class_name, room_size, url, path, datetime, end, js)
         *   .done(function (e) {
         *       return e;
         *   });
         * !!END ORIGIONAL CODE!!
        */

        if (checked) {
            $('#addAppointment').dialog('close');
        }
    }

    adobeConnect.client.addSelf = function (add, event, max, jsHandle) {
        //less or equal
        var html2 = null;
        $('#createAppointment').attr("disabled", false).css('opacity', 1);
        if (max <= 0) { max = 0; }
        var html = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong>  A maximum of <b> " + max + "</b> occupants <u>including the host</u> are available." + "</div>";
        html2 = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> Beware of meetings after you </div>";
        // $("#AppointMent_Submit").prop("disabled", true);
        // ("addAppointment ").dialog.
        if (event.roomSize > max) {
            $('#create').attr("disabled", true).css('opacity', 0.5);
            $('#createAppointment').attr("disabled", true).css('opacity', 0.5);
            alert("Error, that number is too large");
            if (jsHandle) {
                var msg = "Event: " + event.title + " update failed. A maximum of " + max + " participants are avaliable for this time period!";
                notifier(false, "Updating", msg, rt, null, 'error');
            }
            //alert-warning
            /// @TODO:  add message if the events overlap and there is issues with it.
            // html = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> Seats are filled or you are over the alloted maximum of <b>" + max + "</b>.  You might be overlaping with another class</div>";
            // html2 = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> There's a meeting right after you please log off in time </div>";
        }
        $('#error').html(html);
        $('#meetingCrashError').html(html2);
        if (add) {
            notifier(false, "Creating", "Event: " + event.title + " successfully created", null, null, 'success');
            $('#calendar').fullCalendar('renderEvent', event, true);

        }
        // check if there's other events that overlap one another
        //check for 0 execption
        if (event.roomSize < max) {
            $('#create').attr("disabled", false);
            $('#createAppointment').attr("disabled", false); //.css('opacity', 0.5); ungray items
            if (jsHandle) {
                IsUpdate = true;
                addAppointment(true, IsUpdate, false, event);
            }
            //alert("Error, that number is too large number 2 test test");
        }

    }

    getDuration = function (start, end) {
        start = new Date(start);
        end = new Date(end);
        return (parseInt(end - start) / (1000 * 60));
    }
    /*
     * Purpose : updates a multi-day event 
     *
     * Parameters : isUpdate : the parameter permiting event creation or update
     *
     * Returns : void
     */
    updateOrCreate = function (currUpdate, update) {
        if (update == undefined) {
            update = false;
        }
        IsUpdate = update;
        //if the selected DOM object is none, send false, otherwise true
        var isMultiple = ($('#repitition option:selected').text() === "None" ? false : true);

        //function used to figure out if we are a rep event
        var repeatedEvent = numOfRepEvents();

        //if we are indeed a multiple event
        if (isMultiple === true && repeatedEvent.isMult === true) {
            //if there is only one event being created, alert the user
            if (repeatedEvent.numEvents === 1) {
                //if this event is being updated
                if (currUpdate)
                    notifier(false, "Updating", "Updating single instance", null, null, 'success');
                //if not, set the type to no repitition, and continue
                else {
                    notifier(false, "Creating", "Creating single instance, date range provided forcing single instance creation", null, null, 'success');
                    //$('#repitition option:selected').attr("selected", null);
                    $('#repitition').val("None");
                    //$('#repitition option:selected').se = "None";
                }
            }
            else {
                if (currUpdate)
                    notifier(false, "Updating", "Updating all instances", null, null, 'success');
                else
                    notifier(false, "Creating", "Creating event series", null, null, 'success');
            }

            var start = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a');

            //add the events in repitition
            for (var i = 0; i < repeatedEvent.numEvents; i++) {
                //add the event
                addAppointment(true);
                //increment the date
                $('#datetime').val(moment(start.add(repeatedEvent.repType, 'weeks')).format("MM/DD/YYYY hh:mm A "));
            }
            //if this event is being updated
            if (currUpdate)
                notifier(false, "Finished", "Update complete", null, null, 'success');
            //if not, set the type to no repitition, and continue
            else
                notifier(false, "Finished", "Creation is complete", null, null, 'success');
        }
        else {
            //IsUpdate = true;
            addAppointment(true, update);
            //alert("Appointment sucessfuly created.");
        }
    }

    /*
     * Purpose : dictates if an event is a repeatable event
     * 
     * Parameters : event : the appointmenet model
     *
     * Returns : bool isMult: is the incoming event a milti-event, 
     *           int numEvents: the current count of the multi-event appointment,
     *           int repType: the type of repetition {weekly, biweekly, monthly}
     */
    numOfRepEvents = function () {
        //the variable that holds the end moment
        var endMoment = moment($("#repitition_date").val(), 'MM/DD/YYYY hh:mm:ss a');
        //variables which will be returned
        var numEvents = 0;
        var repType = 0;
        var isMult = false;
        //if the selected repitition is set to something and the end moment is atleast a week away
        if ($("#repitition option:selected").text() != "None" && endMoment != null) {
            if (endMoment.add(1, 'weeks') > moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a')) {
                //the temporary moment adn its clone
                var tempMoment = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a');
                var tmpClone = tempMoment.clone();
                //end plus one
                var endClone = endMoment.clone();
                //the number of events which will be generated
                var numberOfEvents = 0;
                //loop through until we are at end date, increment the number of events encountered
                for (tempMoment; endMoment > tmpClone.add(1, 'weeks'); tempMoment.add(1, 'weeks'), tmpClone = tempMoment.clone()) {
                    numberOfEvents++;
                }

                //check for the number of events to be created
                if ($("#repitition option:selected").text() === "Weekly") {
                    numEvents = Math.ceil(numberOfEvents);
                    repType = 1;
                }
                else if ($("#repitition option:selected").text() === "Biweekly") {
                    numEvents = Math.ceil(numberOfEvents / 2);
                    repType = 2;
                }
                else if ($("#repitition option:selected").text() === "Monthly") {
                    numEvents = Math.ceil(numberOfEvents / 4);
                    repType = 4;
                }
                else {
                    numEvents = null;
                }

                //this should never not be evaluated to true. Simple fail safe
                if (numEvents != null) {
                    isMult = true;
                }

                //return this amazing object
                return {
                    isMult: isMult,
                    numEvents: numEvents,
                    repType: repType
                };
            }
        }
        //otherwise, return the object
        else {
            //if the option is "None"
            if ($("#repitition option:selected").text() == "None") {
                return {
                    isMult: isMult,
                    numEvents: numEvents,
                    repType: repType
                };
            }
            //otherwise it's an invalid date range
            else {
                alert("Event: Has an invalid repeat date range. Creating single instance...");
                return {
                    isMult: isMult,
                    numEvents: numEvents,
                    repType: repType
                };
            }
        }

    };

    /*
     * Purpose : generates a random alpha numeric string of length x
     * 
     * Parameters : x : the length of the generated string 
     *
     * Returns : string s : the randomly generated string
    */
    randomString = function (x) {
        var s = "";
        while (s.length < x && x > 0) {
            var r = Math.random();
            s += (r < 0.1 ? Math.floor(r * 100) : String.fromCharCode(Math.floor(r * 26) + (r > 0.5 ? 97 : 65)));
        }
        return s;
    }

    confirmation = function (question) {
        var defer = $.Deferred();
        $('<div></div>')
            .html(question)
            .dialog({
                autoOpen: true,
                modal: true,
                title: 'Confirmation',
                buttons: {
                    "Accept": function () {
                        defer.resolve("true");//this text 'true' can be anything. But for this usage, it should be true or false.
                        $(this).dialog("close");
                    },
                    "Deny": function () {
                        defer.resolve("false");//this text 'false' can be anything. But for this usage, it should be true or false.
                        $(this).dialog("close");
                    }
                }
            });
        return defer.promise();
    }

    adobeConnect.client.callUpdate = function (event) {
        addAppointment(true, true, true, event);
    }

    adobeConnect.client.date = function (date) {
    }

    adobeConnect.client.updateSelf = function (event) {
        notifier(false, "Updating", "Event #" + event.id + ": " + event.title + " successfully updated", null, null, 'success');
        $('#calendar').fullCalendar('removeEvents', event.id);
        $('#calendar').fullCalendar('renderEvent', event, true);
    }

    adobeConnect.client.removeSelf = function (id) {
        $('#calendar').fullCalendar('removeEvents', id)
    }


    adobeConnect.client.addEvent = function (event, checked, isUpdate, jsHandle) {
        adobeConnect.server.addSelf(event, $('#content').attr('data-userId'), checked, isUpdate, window.max, jsHandle, moment().format("MM/DD/YYYY hh:mm A"))
    }

    notifier = function (pd, title, message, cb, data, type) {
        if (pd) {
            var cur_value = 1,
                progress;
            var loader =new PNotify({
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
        console.log('hello');
        $('#calendar').fullCalendar({
            header: {
                left: 'prev,next today month',
                right: ''
            },
            views: {
                month: {
                    titleFormat: 'YYYY MMMM',

                },
                agendaDay: {
                    titleFormat: 'ddd MM/DD/YYYY',
                }

            },

            //titleFormat: {
            //month: '\'<span class="year">\'yyyy\'</span><span class="month">\'MMMM\'</span>\'',
            //day: '\'<span class="day">\'dddd\'</span>\' MM/DD/yyyy'
            //  month: 'yyyy MMMM',
            //  day: 'dddd MM'
            //},            
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
                if (isLoading) {
                    //$('#busy1').activity();
                }
                else {
                    $('#busy1');
                }
            },
            eventRender: function (event, element, view) {
                var roomHtml;
                console.log(view.name);
                if (view.name == 'month') {
                    roomHtml = "</br><b>Occupants</b>: " + "<u>" + event.roomSize + "</u>";
                    element.find(".fc-content")
                        .append(roomHtml);
                }

                adobeConnect.server.checkHost($('#content').attr('data-userId'), event.title).done(function (e) {
                    if (e && !event.archived) {
                        var html = '<a id="editEvent" href="#' + event.id + '"><i class="ui-icon ui-icon-pencil" style="float:right;"></i></a>';
                        element.find(".fc-title").append(
                            (html));
                    }
                });

                if (view.name == 'agendaDay') {
                    roomHtml = event.title + "  " + event.roomSize;
                    element.find(".fc-title")
                        .html(roomHtml);
                }

            },
            viewRender: function (view) {
                let title = view.title;
                title = title.split(" ");
                console.log(view.title);
                console.log(view.titleFormat);
                let htmlText = '';
                if (view.name === 'month') {
                    htmltext = "<span class='year'>{0}</span><span class='month'>{1}</span>".formatUnicorn(title[0], title[1]);
                }
                else {
                    htmltext = '<span class="day">{0}</span><span class="date">{1}</span>'.formatUnicorn(title[0].toUpperCase(), title[1]);
                }
                $("#fctitle").html(htmltext);
            },
            events: function (start, end, timezone, cb) {
                cb(events);
            },
            dayClick: function (date, /*allDay,*/ jsEvent, view) {
                if (view.name == 'month') {
                    $('#calendar').fullCalendar('changeView', 'agendaDay');
                    $('#calendar').fullCalendar('gotoDate', date);

                }

                if (view.name == 'agendaDay') {
                    $('#datetime').val(moment(date).format("MM/DD/YYYY hh:mm A "));
                    IsUpdate = false;
                    if (moment().subtract(30, 'minutes') > moment(date)) {                        
                        alert("Events cannot be created in the past."); return;
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
                                        var events = $('#calendar').fullCalendar('clientEvents');
                                        var checkPrevious = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a').subtract(1, "minutes");
                                        var checkFollowing = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a').add(60, "minutes");
                                        events.forEach(function (event) {
                                            var eventtimeend = event.end;
                                            var eventtimestart = event.start;
                                            if (checkPrevious.isSame(eventtimeend)) {
                                                alert("Warning! There's a meeting finishing right before you.");
                                            }
                                            if (checkFollowing.isSame(eventtimestart)) {
                                                alert("Warning! There's a meeting right after you.");

                                                //TODO add rendering component to the creation.
                                                // var html2 = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> There's a meeting right after you please log off in time </div>";
                                                //$('#meetingCrashError').html(html2);
                                            }
                                        });
                                        //Gets index from data object
                                        // var eventtime = events[82].end;
                                        //  var checkPrevious = moment($('#datetime').val()).subtract(1,"minutes");                                                                                             
                                        updateOrCreate(false);
                                    }
                                },
                                {
                                    text: 'Cancel',
                                    click: function () { $(this).dialog("close") }
                                }

                            ]
                    });
                    $('#addAppointment').dialog('open');

                }
            },
            eventClick: function (event, element) {
                if (element.target.tagName == 'I') {
                    window.max = event.roomSize;
                    window.Id = event.id;
                    IsUpdate = true;
                    var cal_hash = element.target.parentElement.hash;
                    $('#class option:selected').text(event.title);
                    $('#datetime').val(moment(event.start).format("MM/DD/YYYY hh:mm A "));
                    $('#repitition_date').val(moment(event.start).format("MM/DD/YYYY hh:mm A "));
                    $('#occupants').val(event.roomSize);
                    //$('#duration option:selected').text(getDuration(event.start, event.end));
                    $('#duration').val(getDuration(event.start, event.end));
                    //var html = "<input type='submit' onclick='delete_confirm()' class='btn btn-danger' style='float:left' value='Delete Appointment' /><input disabled type='submit' onclick='Update()' id='AppointMent_Submit' class='btn btn-success' value='Update Appointment' />";
                    // $('.modal-footer').html(html);
                    $('#addAppointment').dialog({
                        title: "Update/Delete Appointment",
                        buttons: {
                            "delete": {
                                text: 'Delete',
                                class: 'delete',
                                click: function () { delete_confirm() }
                            },
                            "update": {
                                id: 'create',
                                text: 'Update',
                                class: 'update',
                                click: function () { Update() }
                            },
                            "cancel": {
                                text: 'Cancel',
                                click: function () { $(this).dialog('close') }
                            }
                        }
                    });

                    $('#addAppointment').dialog('open');

                    delete_confirm = function () {
                        var id = event.id,
                            title = event.title,
                            isRep = event.isRep;
                        confirmation('Are you sure you want to permanantly delete this appoinment?').then(function (answer) {
                            if (answer == "false") {
                                notifier(false, "Calceled", "Transaction Canceled", null, null, 'success');
                            }
                            else if (answer == "true") {
                                if (isRep === false) {
                                    adobeConnect.server.delete(id, false);
                                    notifier(false, "Deleting", "Event #" + id + ": " + title + " has been deleted", null, null, 'success');
                                    //$('#addAppointment').dialog('close');
                                }
                                else {
                                    confirmation('The selected event is a repeating event, would you like to remove them all?').then(function (answer) {

                                        if (answer == "false") {
                                            adobeConnect.server.delete(id, false);
                                            notifier(false, "Deleting", "Event #" + id + ": " + title + " has been deleted", null, null, 'success');
                                        }
                                        else if (answer == "true") {
                                            adobeConnect.server.delete(id, true);
                                            notifier(false, "Deleting", "Event #" + id + ": " + title + " and all other events in the series has been deleted", null, null, 'success');
                                        }
                                        //$('#addAppointment').dialog('close');
                                    });
                                    //$('#addAppointment').dialog('close');
                                }
                                $('#addAppointment').dialog('close');
                            }

                        });
                    }

                    Update = function () {
                        //if the current repitition is not "None"
                        var id = event.id,
                            title = event.title;
                        //if an event is none and the ui is none
                        if (!($('#repitition option:selected').text() === "None" && event.repititionType === "None")) {
                            //if we are changing a non repeating to a repeating
                            if (event.repititionType === "None") {
                                if ($('#repitition option:selected').text() != event.repititionType) {
                                    //set isUpdate to false
                                    IsUpdate = false;
                                    //delete the origional
                                    adobeConnect.server.delete(id, false);
                                    //recreate it with it's repitition
                                    updateOrCreate(true);
                                }
                                else {
                                    //set is update to true
                                    IsUpdate = true;
                                    //modify this one event
                                    addAppointment(true, true);
                                }
                                notifier(false, "Updated", "Event #" + id + ": " + title + " has been updated", null, null, 'success');
                            }
                            //otherwise its a repeating event
                            else {
                                confirmation('The selected event is a repeating event, would you like to modify the entire series?').then(function (answer) {
                                    if (answer == "false") {
                                        if ($('#repitition option:selected').text() != event.repititionType) {
                                            //set isUpdate to false
                                            IsUpdate = false;
                                            //delete only one instance of this
                                            adobeConnect.server.delete(id, false);
                                            //update or create new appointment
                                            updateOrCreate(true);
                                        }
                                        else {
                                            //set is update to true
                                            IsUpdate = true;
                                            //modify this one event
                                            addAppointment(true, true);
                                        }

                                        notifier(false, "Updated", "Event #" + id + ": " + title + " has been updated", null, null, 'success');
                                    }
                                    else if (answer == "true") {
                                        if ($('#repitition option:selected').text() != event.repititionType) {
                                            //set isUpdate to false
                                            IsUpdate = false;
                                            //delete the entire series
                                            adobeConnect.server.delete(id, true);
                                            //re-create with the new parameters
                                            updateOrCreate(true);
                                        }
                                        else {
                                            //set is update to true
                                            IsUpdate = true;
                                            //modify all events in the series
                                            addAppointment(true, true, undefined, undefined, true);
                                        }
                                        notifier(false, "Updated", "Event #" + id + ": " + title + " and all other events in the series has been updated", null, null, 'success');
                                    }
                                    $('#addAppointment').dialog('close');
                                });
                            }
                        }
                        else {
                            IsUpdate = true;
                            addAppointment(true, true);
                        }
                    }


                }
                else {
                    if (event.url) {
                        adobeConnect.server.login($('#content').attr('data-userId')).done(function (res) {
                            if (res != "") {
                                $('#request').html("<iframe src='" + res + "'" + " ></iframe>");
                                setTimeout(function () {
                                    $('#loginform').submit();
                                }, 100);

                            } else {
                                html = "<div class='alert alert-error'><button type='button' class='close' data-dismiss='alert'>×</button><strong style='float:left;'>Error!</strong> Invalid Credentials </div>";
                                $('#error').html(html);
                            }

                        });
                        window.open(event.adobeUrl, event.title);
                    }
                }
                return false;
            },
            eventDrop: function (event, dayDelta, minuteDelta, allDay, revertFunc, jsEvent, ui, view) {
                if (event.start < Date.now()) {
                    revertFunc();
                    alert("Events cannot be moved to the past");
                    return false;
                }
                window.Id = event.id;
                IsUpdate = true;
                addAppointment(false, true, true, event);
                notifier(false, "Updating", "Event: " + event.title, null, null, 'info');
                rt = revertFunc;

            },
            eventDragStart: function (event, jsEvent, ui, view) {
            },
            eventDragStop: function (event, jsEvent, ui, view) {
            },
            eventResize: function (event, dayDelta, minuteDelta, revertFunc, jsEvent, ui, view) {
                window.Id = event.id;
                IsUpdate = true;
                if (getDuration(event.start, event.end) > 90) {
                    revertFunc();
                    alert("Events Cannot be longer than 90 miniutes");
                    return false;
                }
                if (event.start < Date.now()) {
                    alert("Events cannot be moved to the past");
                    revertFunc();
                    return false;
                }
                addAppointment(false, true, true, event);
                notifier(false, "Updating", "Event: " + event.title, null, null, 'info');
                rt = revertFunc;

            }

        });


    }
    $.connection.hub.start().done(function () {
        adobeConnect.server.getAllAppointments(moment().format("MM/DD/YYYY hh:mm A")).done(Calendar);
        adobeConnect.server.getAllRooms().done(function (result) {
            if (result.length != null) {
                $('#class').find('option').remove().append('<option selected="selected" disabled="disabled" data-path="" data-url="">Select Your Room:</option>');
                for (var i = 0; i < result.length; i++) {
                    $('#class').append("<option data-path=\"" + result[i][1] + "\" data-url=\"" + "http://turner.southern.edu" +
                        result[i][1] + "\">" + result[i][0] + "</option>");
                }
                $(".chosen-select").trigger("chosen:updated");
            }
        });
    });




    $('.numbersOnly').keyup(function () {
        this.value = this.value.replace(/[^0-9\.]/g, '');
    });

    $('#occupants').keyup(function (e) {
        if ($('#duration option:selected').text() != '' && $('#occupants').val() != '') {
            addAppointment(false, IsUpdate);
        }
    });

    $('#duration').on('change', function () {
        if ($('#duration option:selected').text() != '' && $('#occupants').val() != '') {
            addAppointment(false, IsUpdate);
        }
    });


    // DateTimePicker Set Up
    $('#datetime').datetimepicker({
        minDate: 0,
        timeFormat: "hh:mm TT",
        minuteGrid: 10
    });

    $('#datetime').on('hide', function () {
        addAppointment(false, IsUpdate);
    });

    $('#repitition_date').datetimepicker({
        minDate: 0,
        timeFormat: "hh:mm TT",
        minuteGrid: 10
    });

    $('body').on('click', '.ui-datepicker-close', function () {
        addAppointment(false, IsUpdate);
    });

    $('#class').on('blur', function (e) {
        if ($('#duration option:selected').text() != '' && $('#occupants').val() != '') {
            addAppointment(false, IsUpdate);
        }
    });
});

