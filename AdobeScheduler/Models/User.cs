using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace AdobeScheduler.Models
{
    public class LoginUser 
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }

    [Serializable]
    public class UserSession
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public Models.MeetingItem[] MyMeetings {get; set;}

        public UserSession(Models.MeetingItem[] MeetingItems, UserInfo UserInfo)
        {
            this.FullName = UserInfo.name;
            this.MyMeetings = MeetingItems;
            this.Id = int.Parse(UserInfo.user_id);
        }
    }

    public class ViewObject
    {
        public UserSession Session { get; set; }
        public List<Appointment> AppointmentList { get; set; }
        public Appointment Appointment { get; set; }

        public ViewObject(UserSession session, List<Appointment> appointment)
        {
            this.Session = session;
            this.AppointmentList = appointment;
        }

        public ViewObject(UserSession session, Appointment appointment)
        {
            this.Session = session;
            this.Appointment = appointment;
        }

        public ViewObject(UserSession session)
        {
            this.Session = session;
        }
    }

}