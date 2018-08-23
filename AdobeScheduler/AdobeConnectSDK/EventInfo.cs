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
    /// Event information 
    /// </summary>
    [Serializable]
    public class EventInfo
    {
        [XmlAttribute("sco-id")]
        public string sco_id;

        [XmlAttribute("tree-id")]
        public int tree_id;

        //[XmlAttribute("type")]
        //public string type;

        [XmlAttribute("permission-id")]
        public PermissionID permission_id;


        [XmlElement]
        public string name;

        [XmlElement("domain-name")]
        public string DomainName;

        [XmlElement("date-begin")]
        public DateTime date_begin;

        [XmlElement("date-end")]
        public DateTime date_end;

        [XmlElement("url-path")]
        public string url_path;

        [XmlElement]
        public bool expired;

        [XmlElement]
        public TimeSpan duration;
    }
}
