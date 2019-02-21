using AdobeConnectSDK;
using AdobeConnectSDK.Model;
using AdobeScheduler.Models;
using AdobeScheduler.Util;
using AutoMapper;
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
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using MeetingItem = AdobeScheduler.Models.MeetingItem;

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

    public enum Repetition { None, Weekly, Biweekly, Monthly }

    public class Series
    {
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
    }

    [HubName("adobeConnect")]
    public class AdobeStream : Hub
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        /// <summary>
        /// AddAppointment
        /// </summary>        
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
        /// <param name="serieOperation"></param>
        /// <returns></returns>
        //public bool AddAppointment(bool isUpdate, string roomId, string userId, string name, string roomSize, string url, string path, string JsdateTime, string Jsmin, bool isMultiple, string repId, string JSendRepDate, string repType, bool changeAll)

        private Repetition GetRepetitionEnum(string repetitionType)
        {
            Repetition repetition;

            bool enumParseResult;

            enumParseResult = Enum.TryParse(repetitionType, out repetition);

            if (!enumParseResult)
            {
                repetition = Repetition.None;
            }
            return repetition;
        }

        public bool SaveOrUpdate(Appointment appointment)
        {
            if (appointment.start < DateTime.Now.ToUniversalTime().Subtract(new TimeSpan(0, 30, 0)))
            {
                return false;
            }

            var repetition = GetRepetitionEnum(appointment.repetitionType);

            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                //Abort if there are not enough licenses
                if (repetition == Repetition.None && appointment.id != 0)
                {
                    if (CheckAvailableLicenses(appointment.start, appointment.end, appointment.id) < appointment.roomSize)
                        return false;
                }
                else if (repetition == Repetition.None && appointment.id == 0)
                {
                    if (CheckAvailableLicenses(appointment.start, appointment.end) < appointment.roomSize)
                        return false;
                }
                else if (repetition != Repetition.None)
                {
                    if (CheckAvailableLicenses(appointment.start, appointment.end, repetition, appointment.endRepDate.Value, appointment.repetitionId) < appointment.roomSize)
                        return false;
                }
                
                var start = appointment.start;
                var end = appointment.end;
                var endRepetition = appointment.endRepDate;

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        List<Appointment> seriesToDelete = new List<Appointment>();
                        List<Appointment> insertedItems = new List<Appointment>();
                        if (appointment.id != 0)
                        {                           
                            List<Series> newSeriesDates = new List<Series>();

                            if (repetition == Repetition.None)
                            {
                                var session = (from appointments in _db.Appointments where appointments.id == appointment.id select appointments).OrderBy(d => d.start).First<Appointment>();
                                //Chech if the user has permission
                                if (!HasPermission(session.title))
                                {
                                    return false;

                                }
                                
                                session.title = appointment.title;
                                session.roomSize = appointment.roomSize;
                                session.adobeUrl = appointment.adobeUrl;
                                session.url = appointment.url;
                                session.repetitionType = appointment.repetitionType;
                                session.endRepDate = appointment.endRepDate;
                                session.repetitionId = null;
                                session.endRepDate = appointment.endRepDate;
                                session.start = appointment.start;
                                session.end = appointment.end;
                                session.isEditable = true;
                                _db.SaveChanges();
                                transaction.Commit();
                                NotifyInsertAllClients(session);
                                return true;
                            }
                            else
                            {
                                var session = (from appointments in _db.Appointments where appointments.id == appointment.id select appointments).First<Appointment>();
                                seriesToDelete = (from appointments in _db.Appointments where appointments.repetitionId == session.repetitionId select appointments).OrderBy(d => d.start).ToList();

                                var firstOfSerie = seriesToDelete.OrderBy(a => appointment.start).First();
                                start = new DateTime(firstOfSerie.start.Year, firstOfSerie.start.Month, firstOfSerie.start.Day, appointment.start.Hour, appointment.start.Minute, appointment.start.Second);
                                end = new DateTime(firstOfSerie.end.Year, firstOfSerie.end.Month, firstOfSerie.end.Day, appointment.end.Hour, appointment.end.Minute, appointment.end.Second);
                                endRepetition = firstOfSerie.endRepDate;

                                //Remove the series and insert the new one                                
                                _db.Appointments.RemoveRange(seriesToDelete);

                                //Handle as an insert
                                appointment.id = 0;
                            }
                        }

                        //Abort operation if the room does not exist or user does not have permission
                        if (!HasPermission(appointment.title))
                        {
                            return false;

                        }

                        if (repetition != Repetition.None)
                        {
                            var newSeries = CreateSeries(start, end, repetition, endRepetition.Value);
                            int i = 0;
                            var guid = Guid.NewGuid().ToString();
                            


                            foreach (var item in newSeries)
                            {
                                appointment.start = newSeries[i].StartingDate;
                                appointment.end = newSeries[i].EndingDate;
                                appointment.repetitionId = guid;
                                i++;
                                appointment.isEditable = true;
                                _db.Appointments.Add(appointment);
                                _db.SaveChanges();

                                insertedItems.Add(AdobeScheduler.Util.Utilities.Clone<Appointment>(appointment));
                            }
                            transaction.Commit();
                            NotifyDeleteAllClients(seriesToDelete);
                            NotifyInsertAllClients(insertedItems);
                            return true;
                        }
                        else
                        {
                            appointment.repetitionId = null;
                            appointment.isEditable = true;
                            _db.Appointments.Add(appointment);
                            _db.SaveChanges();
                            transaction.Commit();
                            NotifyInsertAllClients(appointment);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error while inserting or updating a single event or a series of events. This may include delete operations as well.", ex);
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        private void NotifyDeleteAllClients(List<Appointment> inputList)
        {
            foreach (var item in inputList)
            {
                Clients.All.RemoveEvent(item.id);
            }
        }

        private void NotifyInsertAllClients(List<Appointment> inputList)
        {
            foreach (var item in inputList)
            {
                var calendarData = ConstructObject(item, item.userId);
                Clients.All.UpdateEvent(calendarData);
            }
        }

        private void NotifyInsertAllClients(Appointment input)
        {
            var calendarData = ConstructObject(input, input.userId);
            Clients.All.UpdateEvent(calendarData);
        }

        public bool HasPermission(string meetingName)
        {
            List<List<string>> list = new List<List<string>>();

            var cookie = Context.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            bool isAdmin = AdobeConnectSDK.Extensions.PrincipalManagement.IsAdmin(adobeObj, authTicket.UserData.Split('|')[0]); ;

            if (isAdmin)
            {
                return true;                
            }
            else
            {
                var rooms = GetUserRooms().Select(a => a.meeting_name);

                if (!rooms.Contains(meetingName))
                {
                    return false;
                }
                else {
                    return true;
                }
            }
        }
    

        /// <summary>
        /// Function that gets and returns all rooms that the current user has access to either as host or administrator.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MeetingItem> GetUserRooms()
        {           
            List<List<string>> list = new List<List<string>>();

            var cookie = Context.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            bool isAdmin = AdobeConnectSDK.Extensions.PrincipalManagement.IsAdmin(adobeObj, authTicket.UserData.Split('|')[0]); ;

            if (isAdmin)
            {
                return Util.Utilities.Adapter<IEnumerable<MeetingItem>, IEnumerable<AdobeConnectSDK.Model.MeetingItem>>(AdobeConnectSDK.Extensions.MeetingManagement.GetSharedList(adobeObj).Result);
                /*
                foreach (var item in rooms)
                {
                    list.Add(new List<string>() { item.MeetingName, item.UrlPath });
                }
                */
            }
            else
            {
                //var rooms = Utilities.Adapter<Models.MeetingItem[], IEnumerable<AdobeConnectSDK.Model.MeetingItem>>(AdobeConnectSDK.Extensions.MeetingManagement.GetHostMeetings(adobeObj).Result);
                //return AdobeConnectSDK.Extensions.MeetingManagement.GetHostMeetings(adobeObj).Result;
                return Util.Utilities.Adapter<IEnumerable<MeetingItem>, IEnumerable<AdobeConnectSDK.Model.MeetingItem>>(AdobeConnectSDK.Extensions.MeetingManagement.GetHostMeetings(adobeObj).Result);
                /*
                foreach (var item in rooms)
                {
                    list.Add(new List<string>() { item.MeetingName, item.UrlPath });
                } 
                */
            }                       
        }

        public List<MeetingInfo> GetRooms() {
            var domain = ConfigurationManager.AppSettings["NetDomain"];
            return GetUserRooms().Select(a => new MeetingInfo() { meetingName = a.meeting_name, url = a.url_path, adobeUrl = "https://"+domain+a.url_path}).ToList();
        }        

        private int MaxConcurrentLicenses(List<Appointment> sessions)
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

        private List<Series> CreateSeries(DateTime startingTime, DateTime endingTime, Repetition repetition, DateTime endRepetition)
        {
            List<Series> series = new List<Series>();
            DateTime currentStartingTime = startingTime;
            DateTime currentEndingTime = endingTime;
            TimeSpan timeSpan = new TimeSpan();
            series.Add(new Series() { StartingDate = currentStartingTime, EndingDate = currentEndingTime });

            switch (repetition)
            {
                case Repetition.Weekly:
                    timeSpan = new TimeSpan(7, 0, 0, 0);
                    break;
                case Repetition.Biweekly:
                    timeSpan = new TimeSpan(14, 0, 0, 0);
                    break;
                default:
                    break;
            }

            if (timeSpan.CompareTo(TimeSpan.Zero) > 0)
            {
                do
                {
                    currentStartingTime = currentStartingTime + timeSpan;
                    currentEndingTime = currentEndingTime + timeSpan;

                    series.Add(new Series() { StartingDate = currentStartingTime, EndingDate = currentEndingTime });

                } while (currentEndingTime.Add(timeSpan) <= endRepetition);
            }
            else
            {
                do
                {
                    currentStartingTime = currentStartingTime.AddMonths(1);
                    currentEndingTime = currentEndingTime.AddMonths(1);

                    series.Add(new Series() { StartingDate = currentStartingTime, EndingDate = currentEndingTime });

                } while (currentEndingTime.AddMonths(1) <= endRepetition);
            }
            return series;
        }

        public int CheckAvailableLicenses(DateTime startingTime, DateTime endingTime, Repetition repetition, DateTime endRepetition, string repetitionId = "")
        {
            var series = CreateSeries(startingTime, endingTime, repetition, endRepetition);
            var startingDate = series.Select(x => x.StartingDate.Date);
            var endingDate = series.Select(x => x.EndingDate.Date);

            int licensesScheduled = 0;

            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                var sessions = _db.Appointments.Where(a => (startingDate.Contains(DbFunctions.TruncateTime(a.start).Value) || endingDate.Contains(DbFunctions.TruncateTime(a.end).Value))

                && ((DbFunctions.CreateTime(a.start.Hour, a.start.Minute, 0).Value >= DbFunctions.CreateTime(startingTime.Hour, startingTime.Minute, 0) && DbFunctions.CreateTime(a.start.Hour, a.start.Minute, 0) <= DbFunctions.CreateTime(endingTime.Hour, endingTime.Minute, 0))  //start between interval time
                             || (DbFunctions.CreateTime(a.end.Hour, a.end.Minute, 0).Value >= DbFunctions.CreateTime(startingTime.Hour, startingTime.Minute, 0) && DbFunctions.CreateTime(a.end.Hour, a.end.Minute, 0) <= DbFunctions.CreateTime(endingTime.Hour, endingTime.Minute, 0)) //end between interval time
                             || (DbFunctions.CreateTime(a.start.Hour, a.start.Minute, 0).Value <= DbFunctions.CreateTime(startingTime.Hour, startingTime.Minute, 0) && DbFunctions.CreateTime(a.end.Hour, a.end.Minute, 0) >= DbFunctions.CreateTime(endingTime.Hour, endingTime.Minute, 0)) //start earlier and finish later
                             || (DbFunctions.CreateTime(a.start.Hour, a.start.Minute, 0).Value >= DbFunctions.CreateTime(startingTime.Hour, startingTime.Minute, 0) && DbFunctions.CreateTime(a.end.Hour, a.end.Minute, 0) <= DbFunctions.CreateTime(endingTime.Hour, endingTime.Minute, 0)))
                             && ((a.repetitionId != repetitionId && !string.IsNullOrEmpty(repetitionId)) || string.IsNullOrEmpty(repetitionId))
                             ).ToList<Appointment>();

                licensesScheduled = MaxConcurrentLicenses(sessions.ToList<Appointment>());
            }
            int.TryParse(ConfigurationManager.AppSettings["AdobeConnectLicensesAvailable"], out int licenses);
            var availableLicense = (licenses - licensesScheduled);
            return availableLicense;
        }
        //When doing a resize of 25 licenses over a slot that has no licenses available, the application will allow.
        //Strangely enough, it will not allow the resize of a 45 licenses event 
        public int CheckAvailableLicenses(DateTime startingTime, DateTime endingTime, int? eventId = null)
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

                licensesScheduled = MaxConcurrentLicenses(sessions.ToList<Appointment>());
            }
            int.TryParse(ConfigurationManager.AppSettings["AdobeConnectLicensesAvailable"], out int licenses);
            var availableLicense = (licenses - licensesScheduled);
            return availableLicense;
        }

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

                var httpContext = Context.Request.GetHttpContext();
                var cookie = httpContext.Request.Cookies[".ASPXAUTH"];

                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

                CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

                AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

                var test = AdobeConnectSDK.Extensions.PrincipalManagement.GetPrincipalInfo(adobeObj, authTicket.UserData.Split('|')[0]);

                bool isAdmin = AdobeConnectSDK.Extensions.PrincipalManagement.IsAdmin(adobeObj, authTicket.UserData.Split('|')[0]);
                IEnumerable<string> rooms;

                if (isAdmin) {
                    rooms = AdobeConnectSDK.Extensions.MeetingManagement.GetSharedList(adobeObj).Result.Select(a => a.MeetingName);
                }
                else {                    
                    rooms =AdobeConnectSDK.Extensions.MeetingManagement.GetHostMeetings(adobeObj).Result.Select(a => a.MeetingName);                    
                }
                
                foreach (var item in query.Where(a => rooms.Contains(a.title)))
                {
                    item.isEditable = true;
                }

                List<CalendarData> calList = new List<CalendarData>();
                for (var i = 0; i < query.Count; i++)
                {
                    Appointment res = query.ElementAt(i);
                    var obj = ConstructObject(res, HttpContext.Current.User.Identity.Name);
                    calList.Add(obj);
                }
                return await Task.Run(() => calList);
            }
        }

        public CalendarData ConstructObject(Appointment appointment, string id)
        {
            CalendarData calendarData = new CalendarData();
            calendarData.id = appointment.id;
            calendarData.userId = appointment.userId;
            calendarData.title = appointment.title;
            calendarData.url = appointment.url;
            calendarData.color = "#c4afb9";
            calendarData.adobeUrl = appointment.adobeUrl;
            calendarData.roomSize = appointment.roomSize;
            calendarData.start = appointment.start;
            calendarData.end = appointment.end;
            calendarData.editable = appointment.isEditable;
            calendarData.open = true;
            calendarData.archived = false;
            calendarData.repetitionId = appointment.repetitionId;
            calendarData.endRepDate = appointment.endRepDate;
            calendarData.repetitionType = appointment.repetitionType;
            return calendarData;
        }

        public bool Delete(Appointment appointment)
        {
            var rooms = GetUserRooms().Select(a => a.meeting_name);

            //Check if logges users is host in the room
            if (!rooms.Contains(appointment.title))
            {
                return false;
            }

            var repetition = GetRepetitionEnum(appointment.repetitionType);

            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                //querying the data for the population of the calandar object for deletion 
                List<Appointment> query = new List<Appointment>();

                //if we do want to delete all instances of the appointment
                if (repetition != Repetition.None)
                {
                    //holds the initial appointment from which the repId is found
                    List<Appointment> initial = new List<Appointment>();

                    //get the ititial appointment
                    var initialAppointment = (from a in _db.Appointments where a.id == appointment.id select a).FirstOrDefault();

                    //Abort operation user does not have permission
                    if (!HasPermission(initialAppointment.title))
                    {
                        return false;

                    }

                    //get the list of the repeating appointments
                    if (initialAppointment != null)
                    {
                        query = (from a in _db.Appointments where a.repetitionId == initialAppointment.repetitionId select a).ToList();
                    }
                }
                else
                {
                    query = (from a in _db.Appointments where a.id == appointment.id select a).ToList();
                }

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        _db.Appointments.RemoveRange(query);
                        _db.SaveChanges();
                        transaction.Commit();
                        NotifyDeleteAllClients(query);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error while deleting one or a series of events.", ex);
                        return false;
                    }
                }
            }
        }
        /*
        public CalendarData GetEvent(string id, string jsDate)
        {
            int Id = int.Parse(id);
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {

                var query = (from appointmnet in _db.Appointments where appointmnet.id == Id select appointmnet).FirstOrDefault();
                return ConstructObject(query, query.userId);
            }
        }*/

        /*    
        private IEnumerable<string> GetUserHostRooms()
        {
            var httpContext = Context.Request.GetHttpContext();
            var cookie = httpContext.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            return AdobeConnectSDK.Extensions.MeetingManagement.GetMyMeetings(adobeObj).Result.Select(a => a.MeetingName);
        }
        */

        /*   
        private IEnumerable<MeetingItem> GetUserHostRooms()
        {

            var httpContext = Context.Request.GetHttpContext();
            var cookie = httpContext.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            if()

            return Utilities.Adapter<Models.MeetingItem[], IEnumerable<AdobeConnectSDK.Model.MeetingItem>>(AdobeConnectSDK.Extensions.MeetingManagement.GetHostMeetings(adobeObj).Result);
        }*/
        /*
        public bool CheckHost(string username, string meeting)
        {

            var httpContext = Context.Request.GetHttpContext();
            var cookie = httpContext.Request.Cookies[".ASPXAUTH"];

            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(cookie.Value);

            CustomCommunicationProvider customCommunicationProvider = new CustomCommunicationProvider(authTicket.UserData.Split('|')[2], ConfigurationManager.AppSettings["NetDomain"]);

            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI(customCommunicationProvider);

            List<String> meetingList = new List<String>();

            var myMeetings = AdobeConnectSDK.Extensions.MeetingManagement.GetMyMeetings(adobeObj).Result;

            var test = AdobeConnectSDK.Extensions.MeetingManagement.GetAllMeetings(adobeObj);

            foreach (var myMeetingItem in myMeetings)
            {
                meetingList.Add(myMeetingItem.MeetingName);
            }
            var result = meetingList.Contains(meeting);

            return result;
        }*/
    }
}