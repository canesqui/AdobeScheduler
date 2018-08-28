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
    /// SCOshortcut structure
    /// </summary>
    [Serializable]
    public class SCOshortcut
    {
        [XmlAttribute("tree-id")]
        public int tree_id;

        [XmlAttribute("sco-id")]
        public string sco_id;

        [XmlAttribute("type")]
        public string type;

        [XmlElement("domain-name")]
        public string DomainName;

    }
}
