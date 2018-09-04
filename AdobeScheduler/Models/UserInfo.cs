using System;
using System.Xml;
using System.Xml.Serialization;

namespace AdobeScheduler.Models
{
    /// <summary>
    /// UserInfo structure
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        [XmlAttribute("user-id")]
        public string user_id;

        [XmlElement]
        public string name;

        [XmlElement]
        public string login;

        [XmlElement]
        public string sessionInfo;
    }
}
