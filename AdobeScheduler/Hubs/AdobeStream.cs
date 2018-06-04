using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdobeConnectSDK;
using AdobeScheduler.Models;
using System.Globalization;
using System.Web.UI;
using System.Threading.Tasks;

namespace AdobeScheduler.Hubs
{
    public class CalendarData
    {
        public int id {get;set;}
        public string userId { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string adobeUrl { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public bool allDay { get; set; }
        public int roomSize { get; set; }
        public string color {get;set;}
        public bool editable { get; set; }
        public bool open { get; set; }
        public bool archived { get; set; }
        public bool isRep { get; set; }
        public string repititionId { get; set; }
        public DateTime? endRepDate { get; set; }
        public string repititionType { get; set; }


        public CalendarData(){
            this.editable = true;
        }

    }
    
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

        public bool AddAppointment(bool isChecked,bool isUpdate, string roomId, string userId, string name, string roomSize, string url, string path, string JsdateTime, string Jsmin, bool jsHandle)
        {
            
            DateTime date = DateTime.Parse(JsdateTime);
            int endtime = int.Parse(Jsmin);
            DateTime end = date.AddMinutes(endtime);
            if (int.Parse(roomSize) > 50)
            {
                return false;
            }
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
                    if (isChecked)
                    {
                        _db.Appointments.Add(appointment);
                        _db.SaveChanges();
                        Clients.All.addEvent(appointment, isChecked,isUpdate,jsHandle);
                        return true;
                    }
                    else
                    {
                        Clients.Caller.addEvent(appointment, isChecked,isUpdate,jsHandle);
                        return false;
                    }
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
                    if (isChecked)
                    {
                        _db.SaveChanges();
                        Clients.All.addEvent(query, isChecked, true,jsHandle);
                        return true;
                    }
                    else
                    {
                        Clients.Caller.addEvent(query, isChecked,isUpdate,jsHandle);
                        return false;
                    }
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
        public bool AddAppointment(bool isChecked, bool isUpdate, string roomId, string userId, string name, string roomSize, string url, string path, string JsdateTime, string Jsmin, bool jsHandle, bool isMultiple, string repId, string JSendRepDate, string repType, bool changeAll)
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
            if (int.Parse(roomSize) > 50)
            {
                return false;
            }
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                if (!isUpdate)
                {
                    Appointment appointment = new Appointment();
                    CalendarData callendarData = new CalendarData();
                    if (isMultiple)
                    {
                        appointment.userId = userId;
                        appointment.title = name;
                        appointment.roomSize = int.Parse(roomSize);
                        appointment.url = path;
                        appointment.adobeUrl = url;
                        appointment.start = date;
                        appointment.end = end;
                        appointment.endRepDate = endRepTime;
                        appointment.repititionId = repId;
                        appointment.isRep = isMultiple;
                        appointment.repititionType = repType;
                        
                    }
                    else
                    {
                        appointment.userId = userId;
                        appointment.title = name;
                        appointment.roomSize = int.Parse(roomSize);
                        appointment.url = path;
                        appointment.adobeUrl = url;
                        appointment.start = date;
                        appointment.end = end;
                        appointment.endRepDate = date;
                        appointment.repititionId = null;
                        appointment.isRep = isMultiple;
                        appointment.repititionType = repType;
                    }
                    if (isChecked)
                    {
                        _db.Appointments.Add(appointment);
                        _db.SaveChanges();
                        Clients.All.addEvent(appointment, isChecked, isUpdate, jsHandle);
                        return true;
                    }
                    else
                    {
                        Clients.Caller.addEvent(appointment, isChecked, isUpdate, jsHandle);
                        return false;
                    }

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
                        string repititionId = first.repititionId;
                        query = (from appointmnet in _db.Appointments where appointmnet.repititionType == repititionId select appointmnet).ToList();
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

                    if (isChecked)
                    {
                        _db.SaveChanges();
                        foreach (Appointment res in query)
                        {
                            Clients.All.addEvent(res, isChecked, true, jsHandle);
                        }

                        return true;
                    }
                    else
                    {
                        foreach (Appointment res in query)
                        {
                            Clients.Caller.addEvent(res, isChecked, isUpdate, jsHandle);
                        }
                        return false;
                    }
                }
            }

        }
        /// <summary>
        /// Function that gets and returns all rooms
        /// </summary>
        /// <returns></returns>
        public List<List<string>> GetAllRooms()
        {
            LoginInfo login = LoginInfo.currentUser;
            StatusInfo sinfo;
            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI();
            List<List<string>> list = new List<List<string>> { };
            if (adobeObj.Login(login.username, login.password, out sinfo))
            {
                bool isAdmin = adobeObj.IsAdmin(adobeObj.GetUserInfo().user_id);
                if (isAdmin)
                {
                    list = adobeObj.GetSharedList();
                }                
            }
            return list;
        }


        /// <summary>
        /// Initilizes the I frame
        /// </summary>
        /// <returns></returns>
        public string InitilizeIframe()
        {
            LoginInfo login = LoginInfo.currentUser;
            string username = login.username;
            string password = login.password;
            string _targetUrl = string.Format("http://turner.southern.edu/api/xml?action=login&login={0}&password={1}", username, password);
            return _targetUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Login(string username, string password=null)
        {
            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI();
            StatusInfo sInfo;
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                var query = _db.AdobeUserInfo.Where(u => u.Username == username).FirstOrDefault();
                if (password == null)
                {
                    password = query.Password;
                }
                if (adobeObj.Login(username, password, out sInfo) == false)
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
        }

        /// <summary>
        /// An overloaded function which returns a Tuple containing the list of rooms and the 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /*public Tuple<List<List<string>>, string> Login(string username, string password = null)
        {
            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI();
            Tuple<List<List<string>>, string> result = new Tuple<List<List<string>>,string>(null,"");
            StatusInfo sInfo;
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                var query = _db.AdobeUserInfo.Where(u => u.Username == username).FirstOrDefault();
                if (password == null)
                {
                    password = query.Password;
                }
                if (adobeObj.Login(username, password, out sInfo) == false)
                {
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    { return result; }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    string _targetUrl = string.Format("http://turner.southern.edu/api/xml?action=login&login={0}&password={1}", username, password);
                    List<List<string>> list = null;
                    bool isAdmin = adobeObj.IsAdmin(adobeObj.GetUserInfo().user_id);
                    if (isAdmin)
                    {
                        list = adobeObj.GetSharedList();
                    }

                    result = new Tuple<List<List<string>>, string>(list, _targetUrl);

                    return result;
                }
            }
        }*/

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
                    if (appoinment.id != data.id) {
                        selfTotal += appoinment.roomSize;
                    }
                }

                var calendarData = ConstructObject(data, id,jsDate);
                remaining = 50 - selfTotal;
                if (isUpdate) {
                    if (isChecked)
                    {
                        Clients.Caller.updateSelf(calendarData);
                        return;
                    }
                }
                if (isChecked)
                {
                    Clients.Caller.addSelf(true, calendarData, remaining,jsHandle);
                    return;
                }
                Clients.Caller.addSelf(false, calendarData, remaining,jsHandle);
                return;
            }           
        }

        async public Task<List<CalendarData>> GetAllAppointments(string jsDate)
        {
            ///get calendar list data
            /*
            DateTime Date = DateTime.Parse(jsDate);
            DateTime DateS = Date.AddHours(-2);
            DateTime DateM = Date.AddMonths(-1);
            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI();

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
                foreach(Appointment res in query)
                {
                    var obj = ConstructObject(res, HttpContext.Current.User.Identity.Name,jsDate);
                    calList.Add(obj);
                }
                 return await Task.Run(() => calList);
            }
            */
            return null;
        }

        public CalendarData ConstructObject(Appointment appointment, string id, string jsDate)
        {
            Clients.Caller.date(jsDate);
            DateTime Date = DateTime.Parse(jsDate);
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
            callendarData.repititionId = appointment.repititionId;
            callendarData.endRepDate = appointment.endRepDate;
            callendarData.repititionType = appointment.repititionType;

            if (!checkHost(id,callendarData.title))
            {
                callendarData.color = "#d3bf96";
                callendarData.url = "";
                callendarData.editable = false;
            }

            if (checkHost(id, callendarData.title))
            {
                if (Date < callendarData.start)
                {
                    callendarData.color = "#bac7c3";
                    callendarData.open = false;
                    callendarData.url = "";
                }

            }

            if (Date > callendarData.end)
            {
                callendarData.color = "gray";
                callendarData.open = false;
                callendarData.url = "";
                callendarData.editable = false;
                callendarData.archived = true;
            }
            return callendarData;
        }
       
        public bool Delete(string id)
        {
            int Id = int.Parse(id);
            using (AdobeConnectDB _db =  new AdobeConnectDB()){

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
                foreach (Appointment res in query){
                    _db.Appointments.Remove(res);
                }
                if (_db.SaveChanges()>0)
                {
                    Clients.All.removeSelf(Id);
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
                        string repititionId = initial[0].repititionId;
                        query = (from appointmnet in _db.Appointments where appointmnet.repititionId == repititionId select appointmnet).ToList();
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
                    foreach(Appointment identify in query)
                        Clients.All.removeSelf(identify.id);
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
                return ConstructObject(query, query.userId,jsDate);
            }
        }

        public bool checkHost(string username, string meeting)
        {
            AdobeConnectXmlAPI adobeObj = new AdobeConnectXmlAPI();
            StatusInfo sInfo;

            using (AdobeConnectDB _db = new AdobeConnectDB())
            {
                LoginUser query;
                try{
                    query = _db.AdobeUserInfo.Where(u => u.Username == username).FirstOrDefault();
                }
                catch (Exception e)
                {
                    throw e;
                }
                List<String> meetingList = new List<String>();
                if (adobeObj.Login(username, query.Password, out sInfo)){
                    var myMeeting = adobeObj.GetMyMeetings();
                    foreach(MeetingItem myMeetingItem in myMeeting){
                        meetingList.Add(myMeetingItem.meeting_name);
                    }
                    var result = meetingList.Contains(meeting);
                    return result;
                }
                return false;
            }
        }
    }
}