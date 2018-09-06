using System;
using System.Xml;
using System.Xml.Serialization;

namespace AdobeConnectSDK
{
    /// <summary>
    /// MeetingUpdateItem structure
    /// </summary>
    [Serializable]
    [XmlRoot("meeting")]
    public class MeetingUpdateItem
    {
        [XmlAttribute("sco-id")]
        public string sco_id;

        [XmlAttribute("folder-id")]
        public string folder_id;

        [XmlElement]
        public string name;

        [XmlElement]
        public string description;

        [XmlElement]
        public string lang;

        [XmlElement("sco-tag")]
        public string sco_tag;

        [XmlElement("date-begin")]
        public DateTime date_begin;

        [XmlElement("date-modified")]
        public DateTime date_modified;

        [XmlElement("date-end")]
        public DateTime date_end;

        [XmlElement]
        public string email;

        [XmlElement("first-name")]
        public string first_name;

        [XmlElement("last-name")]
        public string last_name;

        [XmlElement("url-path")]
        public string url_path;

        [XmlElement]
        public SCOtype type= SCOtype.not_set;
    }

}
