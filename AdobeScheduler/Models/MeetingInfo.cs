using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace AdobeScheduler.Models
{
    [Serializable]
    public class MeetingInfo
    {
        [XmlElement]
        public string meetingName;

        [XmlElement]
        public string url;

        [XmlElement]
        public string adobeUrl;
    }
}