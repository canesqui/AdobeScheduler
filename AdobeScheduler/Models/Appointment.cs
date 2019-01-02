using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdobeScheduler.Models
{
    public class Appointment
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
        public bool isRep { get; set; }        
        /// <summary>
        /// represents the id used for deletion of the series of events
        /// </summary>
        public string repetitionId { get; set; }
        /// <summary>
        /// represents the end date of the repetition
        /// </summary>
        public DateTime? endRepDate { get; set; }
        /// <summary>
        /// represents the repetition type
        /// </summary>
        public string repetitionType { get; set; }


        public Appointment()
        {
            this.allDay = false;
        }
    }

    public class Room
    {
        public int id { get; set; }
        public int userNum { get; set; }
        public virtual ICollection<Appointment> Rooms { get; set; }
    }
}