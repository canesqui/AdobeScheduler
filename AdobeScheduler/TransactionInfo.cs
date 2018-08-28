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
    /// TransactionInfo structure
    /// </summary>
    [Serializable]
    [XmlRoot("row")]
    public class TransactionInfo
    {
        [XmlAttribute("transaction-id")]
        public string transaction_id;

        [XmlAttribute("sco-id")]
        public string sco_id;

        [XmlAttribute("type")]
        public SCOtype type = SCOtype.not_set;

        [XmlAttribute("principal-id")]
        public string principal_id;

        [XmlAttribute("score")]
        public string score;

        [XmlElement]
        public string name;

        [XmlElement]
        public string url;

        [XmlElement]
        public string login;

        [XmlElement("user-name")]
        public string user_name;

        [XmlElement("status")]
        public string status;

        [XmlElement("date-created")]
        public DateTime date_created;

        [XmlElement("date-closed")]
        public DateTime date_closed;
    }
}
