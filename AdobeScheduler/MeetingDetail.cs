/*
Copyright (c) 2007-2009 Dmitry Stroganov (DmitryStroganov.info)
Redistributions of any form must retain the above copyright notice.
 
Use of any commands included in this SDK is at your own risk. 
Dmitry Stroganov cannot be held liable for any damage through the use of these commands.
*/

using System;
using System.Xml;
using System.Xml.Serialization;

namespace AdobeConnectSDK
{
    /// <summary>
    /// Meeting structure
    /// </summary>
    [Serializable]
    [XmlRoot("sco")]
    public class MeetingDetail
    {
        [XmlAttribute("sco-id")]
        public string sco_id;

        [XmlAttribute("account-id")]
        public string account_id;

        [XmlAttribute("folder-id")]
        public string folder_id;

        [XmlAttribute("lang")]
        public string language;


        [XmlElement("date-created")]
        public DateTime date_created;

        [XmlElement("date-modified")]
        public DateTime date_modified;



        [XmlElement("date-begin")]
        public DateTime date_begin;

        [XmlElement("date-end")]
        public DateTime date_end;



        [XmlElement]
        public string name;

        [XmlElement]
        public string description;

        [XmlElement("url-path")]
        public string url_path;

        [NonSerialized]
        public string FullUrl;

        [XmlElement("passing-score")]
        public int passing_score;

        /// <summary>
        /// The length of time needed to view or play the SCO, in milliseconds.
        /// </summary>
        [XmlElement]
        public int duration; 

        [XmlElement("section-count")]
        public int section_count;

    }
}
