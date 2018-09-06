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
    }
}
