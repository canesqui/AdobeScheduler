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
        modal: true
    });

    $('body').on("change", "#class", function () {
        var e = document.getElementById("class");
        var opt = e.options[e.selectedIndex].getAttribute("data-url");
        var linkText = e.value + " Room Link";
        $('#room_link').text(linkText);
        document.getElementById("room_link").href = opt;
    });

    addAppointment = function (isUpdate, jsHandle, event, changeAll) {
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
        var isMultiple = ($('#repetition option:selected').text() === "None" ? false : true);

        //if the event is undefined, check the DOM for the value, otherwise use the event value the event value will always be defined for rep items
        var repId = (event == undefined) ? ((isMultiple === false) ? null : String(moment().format("MM/DD/YYYY hh:mm A") + userId)) : event.repetitionId;

        //if the event is undefined, check the DOM for the value, otherwise use the event value the event value will always be defined for rep items
        var JSendRepDate = (event == undefined) ? ((isMultiple === false) ? $('#datetime').val() : $('#repetition_date').val()) : moment(event.endRepDate).format("MM/DD/YYYY hh:mm A");

        //if the event is undefined, check the event value
        var repType = (event == undefined) ? $('#repetition option:selected').text() : event.repetitionType;

        //if changeAll = undefined, assume false
        if (changeAll === undefined) {
            changeAll = false;
        }
        console.log('Add appointment called!');
        adobeConnect.server.addAppointment(/*checked,*/ isUpdate, roomId, userId, class_name, room_size, url, path, datetime, end, js, isMultiple, repId, JSendRepDate, repType, changeAll)
            .done(function (e) {
                console.log('addAppointment returned from server');
                return e;
            });
        //if (checked) {
        //  $('#addAppointment').dialog('close');
        //}
    }

    adobeConnect.client.UpdateEvent = function (event) {
        console.log('Update callendar called');
        console.log('Update callendar called with arguments ' + event);
        $('#calendar').fullCalendar('removeEvents', event.id);
        $('#calendar').fullCalendar('renderEvent', event, true);
    }
    /*
    adobeConnect.client.addSelf = function (add, event, max, jsHandle) {
        console.log('addSelf called');
        console.log("Add: " + add);
        console.log("Event: " + event);
        console.log("Max: " + max);
        console.log("jsHandle: " + jsHandle);
        console.log('end of parameter');
        //less or equal
        var html2 = null;
        $('#createAppointment').attr("disabled", false).css('opacity', 1);
        if (max <= 0) { max = 0; }
        //var html = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong>  A maximum of <b> " + window.max + "</b> occupants <u>including the host</u> are available." + "</div>";
        //html2 = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> Beware of meetings after you </div>";
        // $("#AppointMent_Submit").prop("disabled", true);
        // ("addAppointment ").dialog.
        console.log(event.roomSize + ' hello');
        if (event.roomSize > max) {
            console.log('roomsize maior');
            $('#create').attr("disabled", true).css('opacity', 0.5);
            $('#createAppointment').attr("disabled", true).css('opacity', 0.5);
            alert("Error, that number is too large");
            if (jsHandle) {
                var msg = "Event: " + event.title + " update failed. A maximum of " + window.max + " participants are avaliable for this time period!";
                notifier(false, "Updating", msg, rt, null, 'error');
            }
            //alert-warning
            /// @TODO:  add message if the events overlap and there is issues with it.
            // html = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> Seats are filled or you are over the alloted maximum of <b>" + max + "</b>.  You might be overlaping with another class</div>";
            // html2 = "<div class='alert alert-info'><strong style='float:left;'> Warning! </strong> There's a meeting right after you please log off in time </div>";
        }
        else {
            console.log('roomsize menor');
            $('#create').attr("disabled", false);
            $('#createAppointment').attr("disabled", false); //.css('opacity', 0.5); ungray items
            if (jsHandle) {
                IsUpdate = true;
                addAppointment(IsUpdate, false, event);
            }
            alert("Error, that number is too large number 2 test test");
        }
        //$('#error').html(html);
        //$('#meetingCrashError').html(html2);
        if (add) {
            notifier(false, "Creating", "Event: " + event.title + " successfully created", null, null, 'success');
            

        }
        // check if there's other events that overlap one another
        //check for 0 execption


    }
    */
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
        var isMultiple = ($('#repetition option:selected').text() === "None" ? false : true);

        //function used to figure out if we are a rep event
        var repeatedEvent = numOfRepEvents();

        //if we are indeed a multiple event
        if (isMultiple === true && repeatedEvent.isMult === true) {
            //if there is only one event being created, alert the user
            if (repeatedEvent.numEvents === 1) {
                //if this event is being updated
                if (currUpdate)
                    notifier(false, "Updating", "Updating single instance", null, null, 'success');
                //if not, set the type to no repetition, and continue
                else {
                    notifier(false, "Creating", "Creating single instance, date range provided forcing single instance creation", null, null, 'success');
                    $('#repetition').val("None");

                }
            }
            else {
                if (currUpdate)
                    notifier(false, "Updating", "Updating all instances", null, null, 'success');
                else
                    notifier(false, "Creating", "Creating event series", null, null, 'success');
            }

            var start = moment($('#datetime').val(), 'MM/DD/YYYY hh:mm:ss a');

            //add the events in repetition
            for (var i = 0; i < repeatedEvent.numEvents; i++) {
                //add the event
                addAppointment();
                //increment the date
                $('#datetime').val(moment(start.add(repeatedEvent.repType, 'weeks')).format("MM/DD/YYYY hh:mm A "));
            }
            //if this event is being updated
            if (currUpdate)
                notifier(false, "Finished", "Update complete", null, null, 'success');
            //if not, set the type to no repetition, and continue
            else
                notifier(false, "Finished", "Creation is complete", null, null, 'success');
        }
        else {
            //IsUpdate = true;
            console.log('Add appointment ' + update);
            addAppointment(update);
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
        var endMoment = moment($("#repetition_date").val(), 'MM/DD/YYYY hh:mm:ss a');
        //variables which will be returned
        var numEvents = 0;
        var repType = 0;
        var isMult = false;
        //if the selected repetition is set to something and the end moment is atleast a week away
        if ($("#repetition option:selected").text() != "None" && endMoment != null) {
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
                if ($("#repetition option:selected").text() === "Weekly") {
                    numEvents = Math.ceil(numberOfEvents);
                    repType = 1;
                }
                else if ($("#repetition option:selected").text() === "Biweekly") {
                    numEvents = Math.ceil(numberOfEvents / 2);
                    repType = 2;
                }
                else if ($("#repetition option:selected").text() === "Monthly") {
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
            if ($("#repetition option:selected").text() == "None") {
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

    /*
    adobeConnect.client.callUpdate = function (event) {
        addAppointment(true, true, event);
    }
 */
    /*
    adobeConnect.client.updateSelf = function (event) {
        notifier(false, "Updating", "Event #" + event.id + ": " + event.title + " successfully updated", null, null, 'success');
        $('#calendar').fullCalendar('removeEvent', event.id);
        $('#calendar').fullCalendar('renderEvent', event, true);
    }
    */
    adobeConnect.client.RemoveEvent = function (id) {
        $('#calendar').fullCalendar('removeEvents', id)
    }

    /*
    adobeConnect.client.addEvent = function (event, checked, isUpdate, jsHandle) {
        console.log('add event called');
        console.log(this.caller);
        console.log("event: " + event);
        //adobeConnect.server.addSelf(event, $('#content').attr('data-userId'), checked, isUpdate, window.max, jsHandle, moment().format("MM/DD/YYYY hh:mm A"))
    }
    */

    notifier = function (pd, title, message, cb, data, type) {
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
        console.log(events);
        //remove loading text, init adobecal
        $('#calendarLoad').remove();
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
            dayClick: function (date, jsEvent, view) {
                if (view.name == 'month') {
                    $('#calendar').fullCalendar('changeView', 'agendaDay');
                    $('#calendar').fullCalendar('gotoDate', date);

                }

                if (view.name == 'agendaDay') {
                    console.log("hi there1");
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
                                            }
                                        });
                                        updateOrCreate(false);
                                    }
                                },
                                {
                                    text: 'Cancel',
                                    click: function () { $(this).dialog("close") }
                                }

                            ]
                    });
                    adobeConnect.server.checkAvailableLicenses(moment(date).format("MM/DD/YYYY hh:mm A "), moment(date).add($('#duration').val(), 'minutes').format("MM/DD/YYYY hh:mm A "), null)
                        .done(function (e) {
                            window.Id = null;
                            window.max = e;
                            $('#occupants').val(e);
                            updateUI(e);
                            $('#addAppointment').dialog('open');
                            console.log('add appointment dialog called event id parameter isnull');
                        });
                }
            },
            eventClick: function (event, element) {
                if (element.target.tagName == 'I') {
                    console.log("tagName ==I");
                    window.max = event.roomSize;
                    window.Id = event.id;
                    IsUpdate = true;
                    console.log("event id" + event.id);
                    var cal_hash = element.target.parentElement.hash;
                    $('#class option:selected').text(event.title);
                    $('#datetime').val(moment(event.start).format("MM/DD/YYYY hh:mm A "));
                    $('#repetition_date').val(moment(event.start).format("MM/DD/YYYY hh:mm A "));
                    $('#duration').val(getDuration(event.start, event.end));
                    $(".chosen-select").trigger("chosen:updated");
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
                    adobeConnect.server.checkAvailableLicenses(moment(event.start).format("MM/DD/YYYY hh:mm A "), moment(event.end).format("MM/DD/YYYY hh:mm A "), event.id)
                        .done(function (e) {
                            window.max = e;
                            $('#occupants').val(event.roomSize);
                            updateUI(e);
                            $('#addAppointment').dialog('open');
                            console.log('check available licenses was called with event.id parameter')
                        });
                    delete_confirm = function () {
                        var id = event.id,
                            title = event.title,
                            isRep = event.isRep;
                        confirmation('Are you sure you want to permanantly delete this appoinment?').then(function (answer) {
                            if (answer == "false") {
                                notifier(false, "Canceled", "Transaction Canceled", null, null, 'success');
                            }
                            else if (answer == "true") {
                                if (isRep === false) {
                                    adobeConnect.server.delete(id, false);
                                    notifier(false, "Deleting", "Event #" + id + ": " + title + " has been deleted", null, null, 'success');
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
                                    });
                                }
                                $('#addAppointment').dialog('close');
                            }

                        });
                    }

                    Update = function () {
                        //if the current repetition is not "None"
                        var id = event.id,
                            title = event.title;
                        //if an event is none and the ui is none
                        console.log($('#repetition option:selected').text());
                        console.log(event.repetitionType);
                        console.log('Event ++');
                        console.log(event);
                        if (!($('#repetition option:selected').text() === "None" && event.repetitionType === "None")) {
                            //if we are changing a non repeating to a repeating
                            if (event.repetitionType === "None") {
                                if ($('#repetition option:selected').text() != event.repetitionType) {
                                    //set isUpdate to false
                                    IsUpdate = false;
                                    //delete the origional
                                    adobeConnect.server.delete(id, false);
                                    //recreate it with it's repetition
                                    updateOrCreate(true);
                                }
                                else {
                                    //set is update to true
                                    IsUpdate = true;
                                    //modify this one event
                                    addAppointment(true);
                                }
                                notifier(false, "Updated", "Event #" + id + ": " + title + " has been updated", null, null, 'success');
                            }
                            //otherwise its a repeating event
                            else {
                                confirmation('The selected event is a repeating event, would you like to modify the entire series?').then(function (answer) {
                                    if (answer == "false") {
                                        if ($('#repetition option:selected').text() != event.repetitionType) {
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
                                            addAppointment(true);
                                        }

                                        notifier(false, "Updated", "Event #" + id + ": " + title + " has been updated", null, null, 'success');
                                    }
                                    else if (answer == "true") {
                                        if ($('#repetition option:selected').text() != event.repetitionType) {
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
                                            addAppointment(true, undefined, undefined, true);
                                        }
                                        notifier(false, "Updated", "Event #" + id + ": " + title + " and all other events in the series has been updated", null, null, 'success');
                                    }
                                    $('#addAppointment').dialog('close');
                                });
                            }
                        }
                        else {
                            IsUpdate = true;
                            addAppointment(true);
                        }
                        $('#addAppointment').dialog('close');
                    }
                }
                else {
                    /*
                    console.log("called here :)");
                    console.log(event.start);
                    console.log(event.end);
                    adobeConnect.server.checkMaximumAvailableLicenses(event.start, event.end)
                        .done(function (e) {
                            console.log("remote done result is ")
                            window.max = e;
                            $('#addAppointment').dialog('open');
                        });
                        */
                    if (event.url) {
                        adobeConnect.server.login($('#content').attr('data-userId')).done(function (res) {
                            if (res != "") {
                                $('#request').html("<iframe src='" + res + "'" + " ></iframe>");
                                setTimeout(function () {
                                    $('#loginform').submit();
                                    console.log('submit button clicked');
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
                addAppointment(true, true, event);
                console.log('addAppointment');
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
                addAppointment(true, true, event);
                console.log('addAppointment');
                notifier(false, "Updating", "Event: " + event.title, null, null, 'info');
                rt = revertFunc;
            }
        });
    }
    $.connection.hub.start().done(function () {
        console.log("Hub start is done");
        adobeConnect.server.getAllAppointments(moment().format("MM/DD/YYYY hh:mm A")).done(Calendar);
        adobeConnect.server.getAllRooms().done(function (result) {
            console.log(moment().format('HH:mm:ss'));
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

    updateUI = function (value) {
        $('#availableLicenses').text(window.max);
        if (value > window.max || value == 0) {
            console.log('value is greater than window.max ' + value + ', ' + window.max);
            $('#createAppointment').attr("disabled", true).css('opacity', 0.5);
            $('#create').attr("disabled", true).css('opacity', 0.5);
        } else {
            console.log('value is less than window.max ' + value + ', ' + window.max);
            $('#createAppointment').attr("disabled", false).css('opacity', 1);
            $('#create').attr("disabled", false).css('opacity', 1);
        }
    }

    $('#occupants').keyup(function () {
        updateUI(this.value);
    });

    inputDataChanged = function () {
        let start = moment($("#datetime").val(), 'MM/DD/YYYY hh:mm A').format("MM/DD/YYYY hh:mm A");
        let end = moment($("#datetime").val(), 'MM/DD/YYYY hh:mm A').add($("#duration").val(), 'minutes').format("MM/DD/YYYY hh:mm A");        
        if ($("#repetition").val() === 'None') {
            checkAvailableLicenses(start, end, window.id, function (e) {
                console.log('Return from callback ' + e);
                window.max = e;
                updateUI($('#occupants').val());
            });            
        }
        else
        {
            console.log('else case inputDataChanged');            
            let endRepetition = moment($("#repetition_date").val(), 'MM/DD/YYYY hh:mm A');
            console.log('after endRepetition' + endRepetition);
            console.log(start);
            console.log(end);
            console.log(endRepetition);
            checkAvailableLicensesRepeat(start, end, $("#repetition").val(),  endRepetition, window.id, function (e) {
                console.log('Return from callback ' + e);
                window.max = e;
                updateUI($('#occupants').val());
            });
            console.log('after checkavailability call');
        }
        console.log('Datetime inside #duration change ' + start);
        console.log('Datatime inside #duration change adding duration ' + end);
        
    };

    $('#duration').on('change', function () {
        /*if ($('#duration option:selected').text() != '' && $('#occupants').val() != '') {
            addAppointment(false, IsUpdate);
        }*/
        inputDataChanged();

        /*
        adobeConnect.server.checkAvailableLicenses(moment($("#datetime").val(), 'MM/DD/YYYY hh:mm A').format("MM/DD/YYYY hh:mm A"), moment($("#datetime").val(), 'MM/DD/YYYY hh:mm A').add($("#duration").val(), 'minutes').format("MM/DD/YYYY hh:mm A"), window.Id)
            .done(function (e) {
                window.max = e;
                console.log('Available licenses returned ' + e);
                updateUI($('#occupants').val());
            });
            */
    });

    // DateTimePicker Set Up
    $('#datetime').datetimepicker({
        minDate: 0,
        timeFormat: "hh:mm TT",
        minuteGrid: 10
    });

    $('#datetime').on('hide', function () {
        console.log('addAppointment');
        inputDataChanged();
    });
    
    checkAvailableLicensesRepeat = function (startingDateTime, endingDateTime, repetition, endRepetition, eventId, callback) {
        adobeConnect.server.checkAvailableLicenses(startingDateTime, endingDateTime, repetition, endRepetition, eventId)
            .done(function (e) {
                //window.max = e;
                //console.log('Available licenses returned ' + e);
                //updateUI($('#occupants').val());
                callback(e);
            });
    };

    checkAvailableLicenses = function (startingDateTime, endingDateTime, eventId, callback) {
        adobeConnect.server.checkAvailableLicenses(startingDateTime, endingDateTime, eventId)
            .done(function (e) {
                //window.max = e;
                //console.log('Available licenses returned ' + e);
                //updateUI($('#occupants').val());
                callback(e);
            });
    };

    $('#repetition_date').on('change', function () {
        console.log('repetition date change');
        inputDataChanged();
    });

    $('#repetition').on('change', function () {
        console.log('repetition change');
        inputDataChanged();
    });

    $('#repetition_date').datetimepicker({
        minDate: 0,
        timeFormat: "hh:mm TT",
        minuteGrid: 10
    });

    $('body').on('click', '.ui-datepicker-close', function () {
        console.log('addAppointment body click');
        inputDataChanged();
    });

    $('#class').on('blur', function (e) {
        if ($('#duration option:selected').text() != '' && $('#occupants').val() != '') {
            console.log('addAppointment blur');
            //addAppointment(false, IsUpdate);
        }
    });
});

