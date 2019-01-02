using AdobeConnectSDK;
using AdobeScheduler.Models;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SAUAdobeConnectSDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace AdobeScheduler.Hubs
{    
    public class CalendarData
    {
        public int id { get; set; }
        public string userId { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string adobeUrl { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public bool allDay { get; set; }
        public int roomSize { get; set; }
        public string color { get; set; }
        public bool editable { get; set; }
        public bool open { get; set; }
        public bool archived { get; set; }
        public bool isRep { get; set; }
        public string repetitionId { get; set; }
        public DateTime? endRepDate { get; set; }
        public string repetitionType { get; set; }


        public CalendarData()
        {
            this.editable = true;
        }

    }

    public class UnhandledExceptionHandlingModule : HubPipelineModule
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            MethodDescriptor method = invokerContext.MethodDescriptor;
            string errorMessage = String.Format("{0}.{1}({2}) threw the following uncaught exception:", method.Hub.Name, method.Name, String.Join(",", invokerContext.Args));
            Log.Fatal(errorMessage, exceptionContext.Error);
        }
    }

    public enum Repetition { Weekly, Biweekly, Monthly }

    [HubName("adobeConnect")]
    public class AdobeStream : Hub
    {
        private class LoginInfo
        {
            public static LoginInfo currentUser;
            public LoginInfo(string un, string pw)
            {
                username = un;
                password = pw;
            }
            public string username { get; set; }
            public string password { get; set; }
        }
        //TODO: Why not send an object?
        public bool AddAppointment(/*bool isChecked,*/ bool isUpdate, string roomId, string userId, string name, string roomSize, string url, string path, string JsdateTime, string Jsmin, bool jsHandle)
        {

            DateTime date = DateTime.Parse(JsdateTime);
            int endtime = int.Parse(Jsmin);
            DateTime end = date.AddMinutes(endtime);

            if (!isUpdate)
            {
                using (AdobeConnectDB _db = new AdobeConnectDB())
                {
                    Appointment appointment = new Appointment();
                    CalendarData callendarData = new CalendarData();
                    appointment.userId = userId;
                    appointment.title = name;
                    appointment.roomSize = int.Parse(roomSize);
                    appointment.url = path;
                    appointment.adobeUrl = url;
                    appointment.start = date;
                    appointment.end = end;
                    //if (isChecked)
                    //{
                    _db.Appointments.Add(appointment);
                    _db.SaveChanges();

                    var calendarData = ConstructObject(appointment, appointment.userId);
                    Clients.All.UpdateEvent(calendarData);
                    return true;
                    /*}
                    else
                    {
                        Clients.Caller.addEvent(appointment, isChecked, isUpdate, jsHandle);
                        return false;
                    }*/
                }
            }
            else
            {
                int Id = int.Parse(roomId);
                using (AdobeConnectDB _db = new AdobeConnectDB())
                {
                    var query = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).Single();
                    query.start = date;
                    query.roomSize = int.Parse(roomSize);
                    query.title = name;
                    query.adobeUrl = url;
                    query.url = path;
                    query.start = date;
                    query.end = end;
                    //if (isChecked)
                    //{
                    _db.SaveChanges();
                    var calendarData = ConstructObject(query, query.userId);
                    Clients.All.UpdateEvent(calendarData);
                    return true;
                    //}
                    /*
                    else
                    {
                        Clients.Caller.addEvent(query, isChecked, isUpdate, jsHandle);
                        return false;
                    }
                    */
                }
            }

        }
        /// <summary>
        /// Overloaded function of AddAppointment
        /// </summary>
        /// <param name="isChecked"></param>
        /// <param name="isUpdate"></param>
        /// <param name="roomId"></param>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="roomSize"></param>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <param name="JsdateTime"></param>
        /// <param name="Jsmin"></param>
        /// <param name="jsHandle"></param>
        /// <param name="isMultiple"></param>
        /// <param name="repId"></param>
        /// <param name="JSendRepDate"></param>
        /// <param name="repType"></param>
        /// <param name="changeAll"></param>
        /// <returns></returns>
        public bool AddAppointment(bool isUpdate, string roomId, string userId, string name, string roomSize, string url, string path, string JsdateTime, string Jsmin, bool jsHandle, bool isMultiple, string repId, string JSendRepDate, string repType, bool changeAll)
        {

            DateTime date = DateTime.Parse(JsdateTime);
            int endtime = int.Parse(Jsmin);
            DateTime end = date.AddMinutes(endtime);
            DateTime endRepTime;
            //if there is no end rep time
            if (JSendRepDate == "")
            {
                endRepTime = end;
            }
            else
            {
                endRepTime = DateTime.Parse(JSendRepDate);
            }
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                if (!isUpdate)
                {
                    Appointment appointment = new Appointment();
                    CalendarData callendarData = new CalendarData();

                    appointment.userId = userId;
                    appointment.title = name;
                    appointment.roomSize = int.Parse(roomSize);
                    appointment.url = path;
                    appointment.adobeUrl = url;
                    appointment.start = date;
                    appointment.end = end;
                    appointment.isRep = isMultiple;
                    appointment.repetitionType = repType;

                    if (isMultiple)
                    {
                        appointment.endRepDate = endRepTime;
                        appointment.repetitionId = repId;
                    }
                    else
                    {
                        appointment.endRepDate = date;
                        appointment.repetitionId = null;
                    }
                    //if (isChecked)
                    //{
                    _db.Appointments.Add(appointment);
                    _db.SaveChanges();
                    // Clients.All.addEvent(appointment, isChecked, isUpdate, jsHandle);
                    var calendarData = ConstructObject(appointment, appointment.userId);
                    Clients.All.UpdateEvent(calendarData);
                    return true;
                    //}
                    //else
                    //{
                    //    Clients.Caller.addEvent(appointment, isChecked, isUpdate, jsHandle);
                    //    return false;
                    //}

                }
                //if it is indeed an update
                else
                {

                    int Id = int.Parse(roomId);
                    List<Appointment> query = new List<Appointment>();

                    //if it is not an update to a series of events
                    if (!changeAll)
                    {
                        query = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).ToList();
                        foreach (Appointment res in query)
                        {
                            res.start = date;
                            res.roomSize = int.Parse(roomSize);
                            res.title = name;
                            res.adobeUrl = url;
                            res.url = path;
                            res.start = date;
                            res.end = end;
                            res.endRepDate = endRepTime;
                        }
                    }
                    //if it is an update to a series of events
                    else
                    {
                        Appointment first = new Appointment();
                        first = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).Single();
                        string repetitionId = first.repetitionId;
                        query = (from appointmnet in _db.Appointments where appointmnet.repetitionType == repetitionId select appointmnet).ToList();
                        foreach (Appointment res in query)
                        {
                            res.start = date;
                            res.roomSize = int.Parse(roomSize);
                            res.title = name;
                            res.adobeUrl = url;
                            res.url = path;
                            res.start = date;
                            res.end = end;
                            res.endRepDate = endRepTime;
                        }
                    }

                    //if (isChecked)
                    //{
                    _db.SaveChanges();
                    foreach (Appointment res in query)
                    {
                        var calendarData = ConstructObject(res, res.userId);
                        Clients.All.UpdateEvent(calendarData);
                        //Clients.All.addEvent(res, isChecked, true, jsHandle);
                    }
                    return true;
                    /*}
                    else
                    {
                        foreach (Appointment res in query)
                        {
                            Clients.All.UpdateCallendar(res);
                            //Clients.Caller.addEvent(res, isChecked, isUpdate, jsHandle);
                        }
                        return false;
                    }*/
                }
            }
        }

        /// <summary>
        /// Function that gets and returns all rooms
        /// </summary>
        /// <returns></returns>
        public List<List<string>> GetAllRooms()
        {
            var cookie = Context.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            LoginInfo login = LoginInfo.currentUser;
            bool isAdmin = AdobeConnectSDK.Extensions.PrincipalManagement.IsAdmin(adobeObj, authTicket.UserData.Split('|')[0]);
            List<List<string>> list = new List<List<string>>();

            if (isAdmin)
            {
                var result = AdobeConnectSDK.Extensions.MeetingManagement.GetSharedList(adobeObj);

                foreach (var item in result.Result)
                {
                    list.Add(new List<string>() { item.MeetingName, item.UrlPath });
                }
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Login(string username, string password = null)
        {
            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI();

            if (!adobeObj.Login(username, password).Result)
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                { return ""; }
                else
                {
                    return "";
                }
            }
            else
            {
                LoginInfo.currentUser = new LoginInfo(username, password);
                string _targetUrl = string.Format("http://turner.southern.edu/api/xml?action=login&login={0}&password={1}", username, password);
                return _targetUrl;
            }
        }

        private int maxConcurrentLicenses(List<Appointment> sessions)
        {
            int licensesUsed = 0;
            var sortedSessions = sessions.OrderBy(x => x.start).ToList();
            List<int> concurrentLicense = new List<int>();
            concurrentLicense.Add(0);
            foreach (var item in sortedSessions)
            {
                licensesUsed += item.roomSize;
                var finishedMeetings = sessions.Where(x => x.end <= item.start).Select(x => x.roomSize).Sum();
                licensesUsed = licensesUsed - finishedMeetings;
                concurrentLicense.Add(licensesUsed);
            }
            return concurrentLicense.Max();
        }

        public int checkAvailableLicenses(DateTime startingTime, DateTime endingTime, Repetition repetition, DateTime endRepetition, int? eventId = null)
        {
            List<DateTime> startingDate = new List<DateTime>();
            List<DateTime> endingDate = new List<DateTime>();
            DateTime currentStartingTime = startingTime;
            DateTime currentEndingTime = endingTime;
            TimeSpan timeSpan = new TimeSpan();
            Debug.WriteLine("starting date {0}", startingTime.TimeOfDay);
            Debug.WriteLine("starting date {0}", endingTime.TimeOfDay);

            while (currentEndingTime <= endRepetition)
            {
                switch (repetition)
                {
                    case Repetition.Weekly:
                        timeSpan = new TimeSpan(7, 0, 0, 0);
                        currentStartingTime = currentStartingTime + timeSpan;
                        currentEndingTime = currentEndingTime + timeSpan;
                        break;
                    case Repetition.Biweekly:
                        timeSpan = new TimeSpan(14, 0, 0, 0);
                        currentStartingTime = currentStartingTime + timeSpan;
                        currentEndingTime = currentEndingTime + timeSpan;
                        break;
                    case Repetition.Monthly:
                        currentStartingTime.AddMonths(1);
                        currentEndingTime.AddMonths(1);
                        break;
                    default:
                        break;
                }
                startingDate.Add(currentStartingTime.Date);
                endingDate.Add(currentEndingTime.Date);                
            }
            
            int licensesScheduled = 0;
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {

                var sessions = _db.Appointments.Where(a => startingDate.Contains(DbFunctions.TruncateTime(a.start).Value) || endingDate.Contains(DbFunctions.TruncateTime(a.end).Value)                 
                && ((a.start.TimeOfDay >= startingTime.TimeOfDay && a.start.TimeOfDay <= endingTime.TimeOfDay) || //start between interval time
                             (a.end.TimeOfDay >= startingTime.TimeOfDay && a.end.TimeOfDay <= endingTime.TimeOfDay) || //end between interval time
                             (a.start.TimeOfDay <= startingTime.TimeOfDay && a.end.TimeOfDay >= endingTime.TimeOfDay) || //start earlier and finish later
                             (a.start.TimeOfDay >= startingTime.TimeOfDay && a.end.TimeOfDay <= endingTime.TimeOfDay))).ToList<Appointment>();
                
                licensesScheduled = maxConcurrentLicenses(sessions.ToList<Appointment>());
            }
            int.TryParse(ConfigurationManager.AppSettings["AdobeConnectLicensesAvailable"], out int licenses);
            var availableLicense = (licenses - licensesScheduled);
            return availableLicense;
        }

        public int checkAvailableLicenses(DateTime startingTime, DateTime endingTime, int? eventId = null)
        {
            int licensesScheduled = 0;
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                var sessions = _db.Appointments
                .Where(a => ((a.start >= startingTime && a.start <= endingTime) || //start between interval time
                             (a.end >= startingTime && a.end <= endingTime) || //end between interval time
                             (a.start <= startingTime && a.end >= endingTime) || //start earlier and finish later
                             (a.start >= startingTime && a.end <= endingTime)) &&  // start and finish between interval time                             
                             (DbFunctions.TruncateTime(a.start) >= DbFunctions.TruncateTime(startingTime)) &&
                             (DbFunctions.TruncateTime(a.end) <= DbFunctions.TruncateTime(endingTime)) &&
                             (a.id != eventId || eventId == null)).ToList<Appointment>();

                licensesScheduled = maxConcurrentLicenses(sessions.ToList<Appointment>());
            }
            int.TryParse(ConfigurationManager.AppSettings["AdobeConnectLicensesAvailable"], out int licenses);
            var availableLicense = (licenses - licensesScheduled);
            return availableLicense;
        }

        /*
        public void addSelf(Appointment data, string id, bool isChecked, bool isUpdate, int max, bool jsHandle, string jsDate)
        {
            int selfTotal = 0;
            int remaining;
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                var query = (from r in _db.Appointments
                             where ((data.start >= r.start && data.start <= r.end) || (data.end >= r.start && data.end <= r.end))
                             select r
                    );

                foreach (Appointment appoinment in query)
                {
                    //Operator seems to go off blanace +=
                    if (appoinment.id != data.id)
                    {
                        selfTotal = +appoinment.roomSize;
                    }
                }

                var calendarData = ConstructObject(data, id);
                remaining = 70 - selfTotal;
                if (isUpdate)
                {
                    if (isChecked)
                    {
                        
                        Clients.All.updateSelf(calendarData);
                        return;
                    }
                }
                /*
                if (isChecked)
                {
                    Clients.Caller.addSelf(true, calendarData, remaining, jsHandle);
                    return;
                }
                Clients.Caller.addSelf(false, calendarData, remaining, jsHandle);
                */
                //return;
                //}
                //}

                //This function gets the calendar list async. 
                //Most inmportant funtion to load appointment objects
                async public Task<List<CalendarData>> GetAllAppointments(string jsDate)
        {
            ///get calendar list data

            DateTime Date = DateTime.Parse(jsDate);
            DateTime DateS = Date.AddHours(-2);
            DateTime DateM = Date.AddMonths(-1);
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                List<Appointment> query = new List<Appointment>();
                //querying the data for the population of the calandar object
                try
                {
                    query = (from r in _db.Appointments where (r.end >= DateS && r.start >= DateM) select r).ToList();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }

                List<CalendarData> calList = new List<CalendarData>();
                for (var i = 0; i < query.Count; i++)
                {
                    //res
                    Appointment res = query.ElementAt(i);
                    var obj = ConstructObject(res, HttpContext.Current.User.Identity.Name);
                    calList.Add(obj);
                }
                //standard for loop is faster
                /*  foreach (Appointment res in query)
                  {
                      var obj = ConstructObject(res, HttpContext.Current.User.Identity.Name,jsDate);
                      calList.Add(obj);
                  }*/
                return await Task.Run(() => calList);
            }
            // return null;
        }

        public CalendarData ConstructObject(Appointment appointment, string id/*, string jsDate*/)
        {
            //Clients.Caller.date(jsDate);
            //DateTime Date = DateTime.Parse(jsDate);
            CalendarData callendarData = new CalendarData();
            callendarData.id = appointment.id;
            callendarData.userId = appointment.userId;
            callendarData.title = appointment.title;
            callendarData.url = appointment.url;
            callendarData.color = "#c4afb9";
            callendarData.adobeUrl = appointment.adobeUrl;
            callendarData.roomSize = appointment.roomSize;
            callendarData.start = appointment.start;
            callendarData.end = appointment.end;
            callendarData.editable = true;
            callendarData.open = true;
            callendarData.archived = false;
            callendarData.isRep = appointment.isRep;
            callendarData.repetitionId = appointment.repetitionId;
            callendarData.endRepDate = appointment.endRepDate;
            callendarData.repetitionType = appointment.repetitionType;
            return callendarData;
        }

        public bool Delete(string id)
        {
            int Id = int.Parse(id);
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {

                //querying the data for the population of the calandar object for deletion 
                List<Appointment> query = new List<Appointment>();
                try
                {
                    query = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).ToList();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }

                //var query = from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet;
                foreach (Appointment res in query)
                {
                    _db.Appointments.Remove(res);
                }
                if (_db.SaveChanges() > 0)
                {
                    Clients.All.RemoveEvent(Id);
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// An overloaded function of delete, handels multiple events
        /// </summary>
        /// <param name="id">The id of the event to be deleted</param>
        /// <param name="response">True if all events are to be deleted, false if one is to be deleted</param>
        /// <returns>True if deletion was sucessful, false if not</returns>
        public bool Delete(string id, bool response)
        {
            int Id = int.Parse(id);
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {

                //querying the data for the population of the calandar object for deletion 
                List<Appointment> query = new List<Appointment>();
                //if we do want to delete all instances of the appointment
                if (response == true)
                {
                    //holds the initial appointment from which the repId is found
                    List<Appointment> initial = new List<Appointment>();
                    //get the ititial appointment
                    try
                    {
                        initial = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).ToList();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }

                    //get the list of the repeating appointments
                    try
                    {
                        string repetitionId = initial[0].repetitionId;
                        query = (from appointmnet in _db.Appointments where appointmnet.repetitionId == repetitionId select appointmnet).ToList();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }
                else
                {
                    try
                    {
                        query = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).ToList();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }


                //iterate through the list of appointments and delete them all
                foreach (Appointment res in query)
                {
                    _db.Appointments.Remove(res);
                }
                //check and see if appointments were deleted
                if (_db.SaveChanges() > 0)
                {
                    //Clients.All.removeSelf(Id);
                    foreach (Appointment identify in query)
                        Clients.All.RemoveEvent(identify.id);
                    return true;
                }

            }
            return false;
        }

        public CalendarData GetEvent(string id, string jsDate)
        {
            int Id = int.Parse(id);
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {

                var query = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).FirstOrDefault();
                return ConstructObject(query, query.userId);
            }
        }

        public bool CheckHost(string username, string meeting)
        {
            var httpContext = Context.Request.GetHttpContext();

            var cookie = httpContext.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            List<String> meetingList = new List<String>();

            var myMeetings = AdobeConnectSDK.Extensions.MeetingManagement.GetMyMeetings(adobeObj).Result;

            foreach (var myMeetingItem in myMeetings)
            {
                meetingList.Add(myMeetingItem.MeetingName);
            }
            var result = meetingList.Contains(meeting);

            return result;
        }
    }
}