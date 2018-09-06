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
    /// PermissionInfo structure
    /// </summary>
    [Serializable]
    public class PermissionInfo
    {
        [XmlAttribute("principal-id")]
        public string principal_id;

        [XmlAttribute("has-children")]
        public bool has_children;

        [XmlAttribute("is-primary")]
        public bool is_primary;

        [XmlAttribute("permission-id")]
        public PermissionID permission_id;

        [XmlElement]
        public string login;

        [XmlElement]
        public string name;

        [XmlElement]
        public string description;
    }

    public enum SpecialPermissionID
    {
        view_hidden,
        /// <summary>
        /// The meeting is public, and anyone who has the URL for the meeting can enter the room.
        /// </summary>
        remove,
        /// <summary>
        /// The meeting is protected, and only registered users and accepted guests can enter the room.
        /// </summary>
        denied,
        /// <summary>
        /// The meeting is private, and only registered users and participants can enter the room.
        /// </summary>
    }

    public enum PermissionID
    {
        none,
        /// <summary>
        /// The principal has full access to an account and can create users, view any
        /// folder, or launch any SCO. However, the principal cannot publish content or
        /// act as host of an Acrobat Connect Professional meeting.
        /// </summary>
        admin,
        author,
        learner,
        /// <summary>
        /// The principal can view, but cannot modify, the SCO. The principal can take a
        /// course, attend a meeting as participant, or view a folder’s content.
        /// </summary>
        view,
        /// <summary>
        /// Available for meetings only. The principal is host of a meeting and can
        /// create the meeting or act as presenter, even without view permission on the
        /// meeting’s parent folder.
        /// </summary>
        view_hidden,
        /// <summary>
        /// The meeting is public, and anyone who has the URL for the meeting can enter the room.
        /// </summary>
        public_access,
        /// <summary>
        /// Public, equivalent to Anyone who has the URL for the meeting can enter the room.
        /// </summary>
        host,
        /// <summary>
        /// Available for meetings only. The principal is presenter of a meeting and
        /// can present content, share a screen, send text messages, moderate
        /// questions, create text notes, broadcast audio and video, and push content
        /// from web links.
        /// </summary>
        mini_host,
        /// <summary>
        /// Available for meetings only. The principal does not have participant,
        /// presenter or host permission to attend the meeting. If a user is already
        /// attending a live meeting, the user is not removed from the meeting until the
        /// session times out.
        /// </summary>
        remove,
        /// <summary>
        /// Available for SCOs other than meetings. The principal can publish or
        /// update the SCO. The publish permission includes view and allows the
        /// principal to view reports related to the SCO. On a folder, publish does not
        /// allow the principal to create new subfolders or set permissions.
        /// </summary>
        publish,
        /// <summary>
        /// Available for SCOs other than meetings or courses. The principal can
        /// view, delete, move, edit, or set permissions on the SCO. On a folder, the
        /// principal can create subfolders or view reports on folder content.
        /// </summary>
        manage,
        /// <summary>
        /// Available for SCOs other than meetings. The principal cannot view,
        /// access, or manage the SCO.
        /// </summary>
        denied,
        /// <summary>
        /// The meeting is private, and only registered users and participants can enter the room.
        /// </summary>
    }
}
