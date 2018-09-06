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
    /// PrincipalInfo structure
    /// </summary>
    [Serializable]
    public class PrincipalInfo
    {
        public Preferences PrincipialPreferences;
        public Principal PrincipalData;
    }

    /// <summary>
    /// Principal structure
    /// </summary>
    [Serializable]
    [XmlRoot("principal")]
    public class Principal
    {
        [XmlAttribute("account-id")]
        public string account_id;

        [XmlAttribute("principal-id")]
        public string principal_id;

        [XmlAttribute("has-children")]
        public bool has_children;

        [XmlAttribute("is-hidden")]
        public bool is_hidden;

        [XmlAttribute("is-primary")]
        public bool is_primary;

        [XmlElement("ext-login")]
        public string ext_login;

        [XmlElement]
        public string login;

        [XmlElement]
        public string name;

        [XmlElement]
        public string email;

        [XmlElement("first-name")]
        public string first_name;

        [XmlElement("last-name")]
        public string last_name;
    }


    /// <summary>
    /// Preferences structure
    /// </summary>
    [Serializable]
    public class Preferences
    {
        [XmlAttribute("acl-id")]
        public string acl_id;

        [XmlAttribute("lang")]
        public string language;

        [XmlAttribute("time-zone-id")]
        public string time_zone_id;  

    }


    [Serializable]
    public class PrincipalListItem
    {
        [XmlAttribute("account-id")]
        public string account_id;

        [XmlAttribute("principal-id")]
        public string principal_id;

        [XmlAttribute("has-children")]
        public bool has_children;

        [XmlAttribute("is-hidden")]
        public bool is_hidden;

        [XmlAttribute("is-primary")]
        public bool is_primary;

        [XmlElement]
        public string login;

        [XmlElement]
        public string name;

        [XmlElement]
        public string email;
    }

    public enum PrincipalTypes
    {
        admins,
        authors,
        course_admins,
        event_admins,
        event_group,
        everyone,
        external_group,
        external_user,
        group,
        guest,
        learners,
        live_admins,
        seminar_admins,
        user
    }

    /// <summary>
    /// PrincipalSetup structure
    /// </summary>
    public class PrincipalSetup
    {
        /// <summary>
        /// The type of principal. Use only when creating a new principal
        /// </summary>
        public PrincipalTypes type;
        /// <summary>
        /// The principal’s new login name, usually
        /// the principal’s e-mail address. Must be
        /// unique on the server. Required to create
        /// or update a user. Do not use with groups.
        /// </summary>
        public string login;
        /// <summary>
        /// The new group’s name. Use only when
        /// creating a new group. Required to create
        /// a group.
        /// </summary>
        public string name;
        /// <summary>
        /// The user’s new first name. Use only with
        /// users, not with groups. Required to create a user
        /// </summary>
        public string first_name;
        /// <summary>
        /// The new last name to assign to the user.
        /// Required to create a user. Do not use with groups.
        /// </summary>
        public string last_name;
        /// <summary>
        /// The user’s e-mail address. Can be
        /// different from the login. Be sure to
        /// specify a value if you use sendemail=true.
        /// </summary>
        public string email;
        /// <summary>
        /// The new user’s password. Use only when creating a new user.
        /// </summary>
        public string password;
        /// <summary>
        /// The new group’s description. Use only when creating a new group.
        /// </summary>
        public string description;
        /// <summary>
        /// Whether the principal has children. If the
        /// principal is a group, use 1 or true. If the
        /// principal is a user, use 0 or false.
        /// </summary>
        public bool has_children;
        /// <summary>
        /// The ID of the principal that has
        /// information you want to update. Required
        /// to update a user or group, but do not use
        /// to create either.
        /// </summary>
        public string principal_id;
        /// <summary>
        /// A flag indicating whether the server
        /// should send an e-mail to the principal with
        /// account and login information.
        /// </summary>
        public bool send_email;
    }
}
