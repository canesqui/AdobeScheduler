﻿using System;
using System.Linq;
using System.Xml.Serialization;

namespace AdobeScheduler.Models
{
    /// <summary>
    /// MeetingItem structure
    /// </summary>
    [Serializable]
    [XmlRoot("meeting")]
    public class MeetingItem
    {
        [XmlAttribute("sco-id")]
        public string sco_id;

        [XmlAttribute("folder-id")]
        public string folder_id;

        [XmlAttribute("active-participants")]
        public int active_participants;

        [XmlElement]
        public string meeting_name;

        [XmlElement]
        public string meeting_description;

        [XmlElement("lang")]
        public string language;

        [XmlElement("sco-tag")]
        public string sco_tag;

        [XmlElement("domain-name")]
        public string domain_name;

        [XmlElement("url-path")]
        public string url_path;

        [XmlAnyElement]
        public string FullUrl;

        [XmlElement("date-begin")]
        public DateTime date_begin;

        [XmlElement("date-modified")]
        public DateTime date_modified;

        [XmlElement("date-end")]
        public DateTime date_end;

        [XmlElement("is-folder")]
        public bool is_folder;

        [XmlElement]
        public bool expired;

        [XmlElement]
        public TimeSpan Duration;

        [XmlElement("byte-count")]
        public long byte_count;

        [XmlElement]
        public SCOtype type = SCOtype.not_set;        
    }

    public enum SCOtype
    {
        not_set,
        content,
        course,
        curriculum,
        //event,
        folder,
        link,
        meeting,
        session,
        tree
    }

}