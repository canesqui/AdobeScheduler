/*
Copyright 2007-2009 Dmitry Stroganov (DmitryStroganov.info)
Updates and fixes 2010 Project Contributors: scidec
Redistributions of any form must retain the above copyright notice.
 
Use of any commands included in this SDK is at your own risk. 
Dmitry Stroganov cannot be held liable for any damage through the use of these commands.
*/

/*
 * Note: shared conf variables are reqired for this lib to perform propertly 
 * 
<add key="ACxmlAPI_serviceURL" value="https://acrobat.com/api/xml" />
<add key="ACxmlAPI_netUser" value="" />
 * 
 * + proxy optional settings
<settings>
  <ipv6 enabled="true" />
</settings>
<defaultProxy enabled="true" useDefaultCredentials="true">
    <proxy bypassonlocal="True" proxyaddress="http://..." />
</defaultProxy>     
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Configuration;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;


namespace AdobeConnectSDK
{
    /// <summary>
    /// AdobeConnectXmlAPI is intented to work with Adobe Acrobat Professional Enterprise web services.
    /// version supported: from 6 and up
    /// </summary>
    /// 
    [System.Web.AspNetHostingPermission(System.Security.Permissions.SecurityAction.Demand, Level = System.Web.AspNetHostingPermissionLevel.Minimal)]
    public class AdobeConnectXmlAPI
    {
        readonly string m_serviceURL;
        readonly string m_proxyUrl;
        readonly string m_netUser;
        readonly string m_netPassword;
        readonly string m_netDomain;
        readonly bool m_sessionParam;

        string m_SessionInfo = string.Empty;
        string m_SessionDomain = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdobeConnectXmlAPI"/> class.
        /// </summary>
        public AdobeConnectXmlAPI()
            : this(
                ConfigurationManager.AppSettings["ACxmlAPI_serviceURL"],
                ConfigurationManager.AppSettings["ACxmlAPI_netProxyURL"],
                ConfigurationManager.AppSettings["ACxmlAPI_netUser"],
                ConfigurationManager.AppSettings["ACxmlAPI_netPassword"],
                ConfigurationManager.AppSettings["ACxmlAPI_netDomain"],
                ConfigurationManager.AppSettings["ACxmlAPI_UseSessionParameter"])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdobeConnectXmlAPI"/> class.
        /// </summary>
        /// <param name="ServiceURL">URL of your Connect Pro instance</param>
        /// <param name="ProxyUrl">Proxy server used for Internet connectivity</param>
        /// <param name="NetUser">Proxy user</param>
        /// <param name="NetPassword">Proxy password</param>
        /// <param name="NetDomain">Proxy domain</param>
        /// <param name="UseSessionParam">Switch to determine session querystring or session cookie.</param>
        public AdobeConnectXmlAPI(string ServiceURL, string ProxyUrl, string NetUser, string NetPassword, string NetDomain, string UseSessionParam)
        {
            m_serviceURL = "http://turner.southern.edu";
            m_proxyUrl = ProxyUrl;
            m_netUser = NetUser;
            m_netPassword = NetPassword;
            m_netDomain = NetDomain;
            m_sessionParam = true;

            if (string.IsNullOrEmpty(m_serviceURL)) throw new ArgumentNullException("Configuration parameter 'serviceURL' cant be null.");

            //auto-make url conform
            m_serviceURL = m_serviceURL.TrimEnd(new char[] { '/', '?' });
            if (!m_serviceURL.EndsWith("/api/xml"))
            {
                m_serviceURL = m_serviceURL.TrimEnd(new char[] { '/' }) + "/api/xml";
            }
        }


        #region SAUOC : Custom Methods

        /// <summary>
        /// Returns true if user is Admin
        /// </summary>
        /// <param name="acl_id">acl_id of the current user</param>
        /// <returns><see cref="bool"/> bool : user us admin ? true : false</returns>
        public bool IsAdmin(string acl_id)
        {
            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("permissions-info", string.Format("acl-id={0}&filter-type=live-admins", acl_id), out iStatus);
            if (iStatus.Code == StatusCodes.ok && xDoc != null) return true;
            return false;
        }

        /// <summary>
        /// Returns the list of all rooms
        /// </summary>
        /// <remarks This function facilates the need to return the list of all 
        /// urls/rooms for admin view
        /// <returns><see cref="List<List<bool>>"/>List of List of strings {}</returns>
        public List<List<string>> GetSharedList()
        {
            //declare status object to determine if valid
            StatusInfo iStatus;
            //declare results list
            List<List<string>> results = new List<List<string>>();

            //create xDoc based off of processed request (using the meetings sco-id [11002]), terminate if not valid data
            XmlDocument xDoc = _ProcessRequest("sco-expanded-contents", "sco-id=11002", out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            foreach (XmlNode node in xDoc.ChildNodes[1].ChildNodes[1].ChildNodes)
            {
                //add expanded sco-nodes childrens name and url-path attributes to the results list
                if (node.ChildNodes[0].InnerText.IndexOf("/")==-1 && node.Attributes["content-source-sco-icon"].Value == "3")
                {
                    results.Add(new List<string> { node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText });
                }                
            }

            //return the list
            return results;
        }

        #endregion  

        #region Logon, User management


        /// <summary>
        /// Performs log-in procedure
        /// </summary>
        /// <param name="userName">valid Adobe Connect acount name</param>
        /// <param name="userPassword">valid Adobe Connect acount password</param>
        /// <param name="iStatus">after succesfull login, <see cref="StatusInfo">iStatus</see> contains session ID to be used for single-sign-on.</param>
        /// <returns><see cref="bool"/></returns>
        public bool Login(string userName, string userPassword, out StatusInfo iStatus)
        {
            //action=login&login=bobs@acme.com&password=football&session=
            //cookie: BREEZESESSION

            iStatus = new StatusInfo();

            try
            {
                //StatusInfo iStatus;
                XmlDocument xDoc = _ProcessRequest("login", string.Format("login={0}&password={1}", userName, userPassword), true, out iStatus);
                if (xDoc == null || !xDoc.HasChildNodes) return false;
                //return (xDoc.SelectSingleNode("//status/@code").Value.Equals("ok")) ? true : false;
                return (iStatus.Code == StatusCodes.ok) ? true : false;
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }
            return false;

        }

        /// <summary>
        /// Performs log-out procedure
        /// </summary>
        public void Logout()
        {
            //action=logout

            StatusInfo iStatus;
            _ProcessRequest("logout", null, out iStatus);
        }

        /// <summary>
        /// Returns information about currently logged in user
        /// </summary>
        /// <returns><see cref="UserInfo"/></returns>
        public UserInfo GetUserInfo()
        {
            //action=common-info

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("common-info", null, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            try
            {
                if (xDoc.SelectSingleNode("//user") == null) return null;

                UserInfo uInf = new UserInfo();
                uInf.name = xDoc.SelectSingleNode("//user/name/text()").Value;
                uInf.login = xDoc.SelectSingleNode("//user/login/text()").Value;
                uInf.user_id = xDoc.SelectSingleNode("//user/@user-id").Value;

                
                return uInf;
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }

            return null;
        }

        /// <summary>
        /// Provides information about one principal, either a user or a group.
        /// </summary>
        /// <param name="principal_id">*required</param>
        /// <returns><see cref="PrincipalInfo"/></returns>
        public PrincipalInfo GetPrincipalInfo(string principal_id)
        {
            //act: "principal-info"

            if (string.IsNullOrEmpty(principal_id)) throw new ArgumentNullException("principal_id");

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("principal-info", string.Format("principal-id={0}", principal_id), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            try
            {
                PrincipalInfo pInfo = new PrincipalInfo();

                //Preferences
                XmlNode PreferencesNode = xDoc.SelectSingleNode("//preferences");
                if (PreferencesNode == null) return null;

                pInfo.PrincipialPreferences = new Preferences();
                pInfo.PrincipialPreferences.acl_id = PreferencesNode.Attributes["acl-id"].Value;
                pInfo.PrincipialPreferences.language = PreferencesNode.Attributes["lang"].Value;
                pInfo.PrincipialPreferences.time_zone_id = PreferencesNode.Attributes["time-zone-id"].Value;

                PreferencesNode = null;

                //Principal
                XmlNode PrincipalDetailNode = xDoc.SelectSingleNode("//principal");
                if (PrincipalDetailNode == null || !PrincipalDetailNode.HasChildNodes) return null;

                pInfo.PrincipalData = new Principal();
                pInfo.PrincipalData.account_id = PrincipalDetailNode.Attributes["account-id"].Value;
                if (!bool.TryParse(PrincipalDetailNode.Attributes["is-hidden"].Value, out pInfo.PrincipalData.is_hidden))
                    pInfo.PrincipalData.is_hidden = false;
                if (!bool.TryParse(PrincipalDetailNode.Attributes["is-primary"].Value, out pInfo.PrincipalData.is_primary))
                    pInfo.PrincipalData.is_primary = false;
                if (!bool.TryParse(PrincipalDetailNode.Attributes["has-children"].Value, out pInfo.PrincipalData.has_children))
                    pInfo.PrincipalData.has_children = false;

                pInfo.PrincipalData.login = PrincipalDetailNode.SelectSingleNode("login/text()").Value;
                pInfo.PrincipalData.name = PrincipalDetailNode.SelectSingleNode("name/text()").Value;
                pInfo.PrincipalData.email = PrincipalDetailNode.SelectSingleNode("email/text()").Value;

                //these are optional fields so need to check if they exist first
                XmlNode fname = PrincipalDetailNode.SelectSingleNode("first-name/text()");
                if (fname != null) pInfo.PrincipalData.first_name = fname.Value;
                XmlNode lname = PrincipalDetailNode.SelectSingleNode("last-name/text()");
                if (lname != null) pInfo.PrincipalData.last_name = lname.Value;

                PrincipalDetailNode = null;

                return pInfo;
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }
            return null;
        }


        /// <summary>
        /// Provides a complete list of users and groups, as XmlNodeList
        /// </summary>
        public XmlNodeList GetPrincipalListRaw()
        {
            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("principal-list", string.Empty, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList PrincipalNodes = xDoc.SelectNodes("//principal-list/principal");
            if (PrincipalNodes == null || PrincipalNodes.Count < 1) return null;

            return PrincipalNodes;
        }

        /// <summary>
        /// Provides a complete list of users and groups, including primary groups.
        /// </summary>
        public PrincipalListItem[] GetPrincipalList()
        {
            return this.GetPrincipalList(string.Empty, string.Empty);
        }

        /// <summary>
        /// Provides a complete list of users and groups, including primary groups.
        /// </summary>
        /// <param name="group_id">optional</param>
        /// <param name="filterby">optional</param>
        /// <returns><see cref="PrincipalListItem">PrincipalListItem</see> array</returns>
        public PrincipalListItem[] GetPrincipalList(string group_id, string filterby)
        {
            //act: "principal-list"

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("principal-list", string.Format("group-id={0}&{1}", group_id, filterby), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList PrincipalNodes = xDoc.SelectNodes("//principal-list/principal");
            if (PrincipalNodes == null || PrincipalNodes.Count < 1) return null;

            List<PrincipalListItem> piList = new List<PrincipalListItem>();
            foreach (XmlNode node in PrincipalNodes)
            {
                PrincipalListItem pli = new PrincipalListItem();

                try
                {
                    pli.principal_id = node.Attributes["principal-id"].Value;
                    pli.account_id = node.Attributes["account-id"].Value;
                    if (!bool.TryParse(node.Attributes["is-hidden"].Value, out pli.is_hidden))
                        pli.is_hidden = false;
                    if (!bool.TryParse(node.Attributes["is-primary"].Value, out pli.is_primary))
                        pli.is_primary = false;
                    if (!bool.TryParse(node.Attributes["has-children"].Value, out pli.has_children))
                        pli.has_children = false;

                    pli.login = node.SelectSingleNode("login/text()").Value;
                    pli.name = node.SelectSingleNode("name/text()").Value;
                    if (node.SelectSingleNode("email/text()") != null)
                        pli.email = node.SelectSingleNode("email/text()").Value;

                    piList.Add(pli);

                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }
            }

            return piList.ToArray();
        }

        /// <summary>
        /// Removes one or more principals, either users or groups.
        /// To delete principals, you must have Administrator privilege.
        /// To delete multiple principals, specify multiple principal-id parameters. All of the principals
        /// you specify will be deleted.
        /// The principal-id can identify either a user or group. If you specify a user, the user is
        /// removed from any groups the user belongs to. If you specify a group, the group is deleted, but
        /// the users who belong to it are not.
        /// </summary>
        /// <param name="principal_id">*required</param>
        public StatusInfo PrincipalsDelete(string[] principal_id)
        {
            //act: "principals-delete"

            for (int i = 0; i < principal_id.Length; i++)
            {
                principal_id[i] = "principal-id=" + principal_id[i];
            }

            StatusInfo iStatus;
            _ProcessRequest("principals-delete", string.Join("&", principal_id), out iStatus);
            return iStatus;
        }

        /// <summary>
        /// Creates or updates a user or group. The user or group (that is, the principal) is created or
        /// updated in the same account as the user making the call.
        /// </summary>
        public StatusInfo PrincipalUpdate(PrincipalSetup pSetup)
        {
            //action=principal-update

            string _cmdParams = StructToQueryString(pSetup, true);

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("principal-update", _cmdParams, out iStatus);
            return iStatus;
        }

        /// <summary>
        /// Changes a user’s password. A password can be changed in either of these cases:
        ///  By an Administrator logged in to the account, with or without the user’s old password
        ///  By any Connect Enterprise user, with the user’s principal-id number, login name, and old password        
        /// </summary>
        /// <param name="user_id">The ID of the user.</param>
        /// <param name="password_old">The user’s current password. Required for regular users, but not for Administrator users.</param>
        /// <param name="password">The new password.</param>
        public StatusInfo PrincipalUpdatePwd(string user_id, string password_old, string password)
        {
            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("user-update-pwd", string.Format("user-id={0}&password-old={1}&password={2}", user_id, password_old, password), out iStatus);
            return iStatus;
        }

        /// <summary>
        /// Adds one or more principals to a group, or removes one or more principals from a group.
        /// To update multiple principals and groups, specify multiple trios of group-id, principal-id,
        /// and is-member parameters.
        /// </summary>
        /// <param name="group_id">The ID of the group in which you want to add or change members.</param>
        /// <param name="principal_id">The ID of the principal whose membership status you want to update. Returned by principal-info.</param>
        /// <param name="is_member">Whether the principal is added to (true) or deleted from (false) the group.</param>
        public StatusInfo PrincipalGroupMembershipUpdate(string group_id, string principal_id, bool is_member)
        {
            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("group-membership-update", string.Format("group-id={0}&principal-id={1}&is-member={2}", group_id, principal_id, (is_member == true) ? 1 : 0), out iStatus);
            return iStatus;
        }

        /// <summary>
        /// Returns the list of principals (users or groups) who have permissions to act on a SCO,
        /// principal, or account.
        /// </summary>
        /// <param name="acl_id">*Required.
        /// The ID of a SCO, account, or principal
        /// that a principal has permission to act
        /// on. The acl-id is a sco-id, principalid,
        /// or account-id in other calls.
        /// </param>
        /// <param name="principal_id">Optional. 
        /// The ID of a user or group who has a
        /// permission (even if denied or not set) to
        /// act on a SCO, an account, or another principal.
        /// </param>
        /// <param name="filter">optional</param>
        /// <returns><see cref="PermissionInfo">PermissionInfo</see> array</returns>
        public PermissionInfo[] GetPermissionsInfo(string acl_id, string principal_id, string filter)
        {
            //action=permissions-info

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("permissions-info", string.Format("acl-id={0}&principal-id={1}&filter-definition={2}", acl_id, principal_id, filter), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList PrincipalNodes = xDoc.SelectNodes("//permissions/principal");
            if (PrincipalNodes == null || PrincipalNodes.Count < 1) return null;

            List<PermissionInfo> piList = new List<PermissionInfo>();
            foreach (XmlNode node in PrincipalNodes)
            {
                PermissionInfo pli = new PermissionInfo();

                try
                {
                    pli.principal_id = node.Attributes["principal-id"].Value;
                    if (!string.IsNullOrEmpty(node.Attributes["permission-id"].Value))
                        pli.permission_id = (PermissionID)this.ReflectEnum(typeof(PermissionID), node.Attributes["permission-id"].Value);
                    else pli.permission_id = PermissionID.none;
                    pli.principal_id = node.Attributes["principal-id"].Value;

                    if (!bool.TryParse(node.Attributes["is-primary"].Value, out pli.is_primary))
                        pli.is_primary = false;
                    if (!bool.TryParse(node.Attributes["has-children"].Value, out pli.has_children))
                        pli.has_children = false;

                    if (node.SelectSingleNode("login/text()") != null)
                        pli.login = node.SelectSingleNode("login/text()").Value;
                    pli.name = node.SelectSingleNode("name/text()").Value;

                    if (node.SelectSingleNode("description/text()") != null)
                        pli.description = node.SelectSingleNode("description/text()").Value;

                    piList.Add(pli);

                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }
            }

            return piList.ToArray();
        }


        /// <summary>
        /// Resets all permissions any principals have on a SCO to the permissions of its parent SCO. If
        /// the parent has no permissions set, the child SCO will also have no permissions.
        /// </summary>
        /// <param name="acl_id">*required</param>
        public StatusInfo PermissionsReset(string acl_id)
        {
            //act: "permissions-reset"

            StatusInfo iStatus;
            _ProcessRequest("permissions-reset", string.Format("acl-id={0}", acl_id), out iStatus);
            return iStatus;
        }

        /// <summary>
        /// The server defines a special principal, public-access, which combines with values of permission-id to create special access permissions to meetings.
        /// </summary>
        /// <param name="acl_id">*required</param>
        /// <param name="permID">*required</param>
        /// <returns>StatusInfo</returns>
        public StatusInfo SpecialPermissionsUpdate(string acl_id, SpecialPermissionID permID)
        {
            switch (permID)
            {
                case SpecialPermissionID.denied:
                    return PermissionsUpdate(acl_id, "public-access", PermissionID.denied);
                case SpecialPermissionID.remove:
                    return PermissionsUpdate(acl_id, "public-access", PermissionID.remove);
                case SpecialPermissionID.view_hidden:
                    return PermissionsUpdate(acl_id, "public-access", PermissionID.view_hidden);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Updates the permissions a principal has to access a SCO, using a trio of principal-id, aclid,
        /// and permission-id. To update permissions for multiple principals or objects, specify
        /// multiple trios. You can update more than 200 permissions in a single call to permissionsupdate.
        /// </summary>
        /// <param name="acl_id">*required</param>
        /// <param name="principal_id">*required</param>
        /// <param name="permID">*required</param>
        /// <returns>StatusInfo</returns>
        public StatusInfo PermissionsUpdate(string acl_id, string principal_id, PermissionID permID)
        {
            //act: "permissions-update"

            StatusInfo iStatus;
            _ProcessRequest("permissions-update", string.Format("acl-id={0}&principal-id={1}&permission-id={2}", acl_id, principal_id, this.EnumToStr(permID)), out iStatus);
            return iStatus;
        }

        #endregion

        #region Webinars : SET
        /// <summary>
        /// Creates a new meeting.
        /// </summary>
        /// <param name="mItem">MeetingUpdateItem</param>
        /// <param name="mDetail">returns created MeetingDetail</param>
        /// <returns>StatusInfo</returns>
        public StatusInfo CreateMeeting(MeetingUpdateItem mItem, out MeetingDetail mDetail)
        {
            mDetail = null;
            if (mItem == null) return null;
            if (string.IsNullOrEmpty(mItem.folder_id))
            {
                return ReportStatus(StatusCodes.invalid, StatusSubCodes.format, new ArgumentNullException("MeetingItem", "folder_id must be set to create new item"));
                //throw new ArgumentNullException("MeetingItem", "folder_id must be set to create new item");
            }
            if (mItem.type == SCOtype.not_set)
            {
                return ReportStatus(StatusCodes.invalid, StatusSubCodes.format, new ArgumentNullException("MeetingItem", "SCOtype must be set"));
                //throw new ArgumentNullException("MeetingItem", "SCOtype must be set");
            }
            mItem.sco_id = null;

            return this.UpdateSCO(mItem, out mDetail);
        }

        /// <summary>
        /// Updates the meeting.
        /// </summary>
        /// <param name="mItem">MeetingUpdateItem</param>
        /// <returns>StatusInfo</returns>
        public StatusInfo UpdateMeeting(MeetingUpdateItem mItem) //, out MeetingDetail mDetail
        {
            MeetingDetail mDetail = null;
            if (mItem == null) return null;
            if (string.IsNullOrEmpty(mItem.sco_id))
            {
                return ReportStatus(StatusCodes.invalid, StatusSubCodes.format, new ArgumentNullException("MeetingItem", "sco_id must be set to update existing item"));
                //throw new ArgumentNullException("MeetingItem", "sco_id must be set to update existing item");
            }
            mItem.folder_id = null;

            return this.UpdateSCO(mItem, out mDetail);
        }

        /// <summary>
        /// Creates metadata for a SCO, or updates existing metadata describing a SCO.
        /// Call sco-update to create metadata only for SCOs that represent content, including
        /// meetings. You also need to upload content files with either sco-upload or Connect Enterprise Manager.
        /// You must provide a folder-id or a sco-id, but not both. If you pass a folder-id, scoupdate
        /// creates a new SCO and returns a sco-id. If the SCO already exists and you pass a
        /// sco-id, sco-update updates the metadata describing the SCO.
        /// After you create a new SCO with sco-update, call permissions-update to specify which
        /// users and groups can access it.
        /// </summary>
        private StatusInfo UpdateSCO(MeetingUpdateItem mItem, out MeetingDetail mDetail)
        {
            mDetail = null;
            if (mItem == null) return null;

            string _cmdParams = this.StructToQueryString(mItem, true);

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("sco-update", _cmdParams, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return iStatus;

            //notice: no '/sco' will be returned during update
            XmlNode MeetingDetailNode = xDoc.SelectSingleNode("//sco");
            if (MeetingDetailNode == null) return iStatus;

            try
            {
                mDetail = new MeetingDetail();
                mDetail.date_created = DateTime.UtcNow;
                mDetail.date_modified = DateTime.UtcNow;
                mDetail.sco_id = MeetingDetailNode.Attributes["sco-id"].Value;
                mDetail.folder_id = MeetingDetailNode.Attributes["folder-id"].Value;
                mDetail.account_id = MeetingDetailNode.Attributes["account-id"].Value;
                mDetail.url_path = MeetingDetailNode.SelectSingleNode("url-path/text()").Value;
                if (!string.IsNullOrEmpty(mDetail.url_path))
                {
                    Uri u = new Uri(m_serviceURL);
                    mDetail.FullUrl = "http://" + u.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped) + mDetail.url_path;
                }
                mDetail.name = MeetingDetailNode.SelectSingleNode("name/text()").Value;
                if (MeetingDetailNode.SelectSingleNode("description/text()") != null)
                    mDetail.description = MeetingDetailNode.SelectSingleNode("description/text()").Value;
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
                iStatus.Code = StatusCodes.invalid;
                iStatus.SubCode = StatusSubCodes.format;
                iStatus.UndeliningExceptionInfo = ex;
                //delete meeting
                try
                {
                    if (!string.IsNullOrEmpty(mDetail.sco_id))
                        this.DeleteSCO(mDetail.sco_id);
                }
                finally { }
            }

            return iStatus;
        }

        /// <summary>
        /// Deletes one or more objects (SCOs).
        /// If the sco-id you specify is for a folder, all the contents of the specified folder are deleted. To
        /// delete multiple SCOs, specify multiple sco-id parameters.
        /// You can use a call such as sco-contents to check the ref-count of the SCO, which is the
        /// number of other SCOs that reference this SCO. If the SCO has no references, you can safely
        /// remove it, and the server reclaims the space.
        /// If the SCO has references, removing it can cause the SCOs that reference it to stop working,
        /// or the server not to reclaim the space, or both. For example, if a course references a quiz
        /// presentation, removing the presentation might make the course stop working.
        /// As another example, if a meeting has used a content SCO (such as a presentation or video),
        /// there is a reference from the meeting to the SCO. Deleting the content SCO does not free
        /// disk space, because the meeting still references it.
        /// To delete a SCO, you need at least manage permission (see permission-id for details). Users
        /// who belong to the built-in authors group have manage permission on their own content
        /// folder, so they can delete content within it.
        /// </summary>
        /// <param name="sco_id"></param>
        private StatusInfo DeleteSCO(string sco_id)
        {
            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("sco-delete", string.Format("&sco-id=", sco_id), out iStatus);
            return iStatus;
        }

        #endregion

        #region Webinars : GET

        /// <summary>
        /// Provides result from calling GetSCOshotcuts(), 
        /// with conditional filtering applied: scoItem.type.Equals("meetings")
        /// </summary>
        /// <returns><see cref="SCOshortcut">SCOshortcut array</see></returns>
        public SCOshortcut[] GetMeetingShotcuts()
        {
            SCOshortcut[] meetingsc = this.GetSCOshotcuts();
            if (meetingsc == null) return null;

            List<SCOshortcut> miList = new List<SCOshortcut>();
            miList.AddRange(meetingsc);
            meetingsc = null;

            return miList.FindAll(delegate(SCOshortcut scoItem)
            {
                return (scoItem.type.Equals("meetings")) ? true : false;
            }).ToArray();
        }

        /// <summary>
        /// Provides information about the folders relevant to the current user. These include a folder for
        /// the user’s current meetings, a folder for the user’s content, as well as folders above them in the
        /// navigation hierarchy.
        /// To determine the URL of a SCO, concatenate the url-path returned by sco-info, scocontents,
        /// or sco-expanded-contents with the domain-name returned by sco-shortcuts.
        /// For example, you can concatenate these two strings:
        /// - http://test.server.com (the domain-name returned by sco-shortcuts)
        /// - /f2006123456/ (the url-path returned by sco-info, sco-contents, or scoexpanded-contents)
        /// The result is this URL: http://test.server.com/f2006123456/
        /// You can also call sco-contents with the sco-id of a folder returned by sco-shortcuts to
        /// see the contents of the folder.
        /// </summary>
        /// <returns><see cref="SCOshortcut">SCOshortcut array</see></returns>
        public SCOshortcut[] GetSCOshotcuts()
        {
            //act: "sco-shortcuts"
            //ret: "//shortcuts"

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("sco-shortcuts", null, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList scoNodes = xDoc.SelectNodes("//shortcuts/sco");
            if (scoNodes == null || scoNodes.Count < 1) return null;

            List<SCOshortcut> miList = new List<SCOshortcut>();
            foreach (XmlNode node in scoNodes)
            {
                SCOshortcut mi = new SCOshortcut();

                try
                {
                    mi.sco_id = node.Attributes["sco-id"].Value;
                    if (!int.TryParse(node.Attributes["tree-id"].Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out mi.tree_id))
                        mi.tree_id = -1;
                    mi.type = node.Attributes["type"].Value;
                    mi.DomainName = node.SelectSingleNode("domain-name/text()").Value;
                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }

                miList.Add(mi);
            }

            return miList.ToArray();

        }

        /// <summary>
        /// Provides information about all Acrobat Connect meetings for which the user is a host, invited
        /// participant, or registered guest. The meeting can be scheduled in the past, present, or future.
        /// </summary>
        /// <returns>
        /// <see cref="MeetingItem">Meeting list</see>
        /// *Note: all dates are GMT
        /// </returns>
        public MeetingItem[] GetMyMeetings()
        {
            //act: "report-my-meetings"
            //ret: "//meeting"

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("report-my-meetings", null, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList meetingNodes = xDoc.SelectNodes("//my-meetings/meeting");
            if (meetingNodes == null || meetingNodes.Count < 1) return null;

            List<MeetingItem> miList = new List<MeetingItem>();
            foreach (XmlNode node in meetingNodes)
            {
                MeetingItem mi = new MeetingItem();

                try
                {
                    mi.sco_id = node.Attributes["sco-id"].Value;
                    if (!int.TryParse(node.Attributes["active-participants"].Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out mi.active_participants))
                        mi.active_participants = -1;

                    mi.meeting_name = node.SelectSingleNode("name/text()").Value;
                    //mi.description = node.SelectSingleNode("description/text()").Value;
                    if (node.SelectSingleNode("description/text()") != null)
                        mi.meeting_description = node.SelectSingleNode("description/text()").Value;
                    mi.domain_name = node.SelectSingleNode("domain-name/text()").Value;
                    mi.url_path = node.SelectSingleNode("url-path/text()").Value;
                    
                    mi.FullUrl = "http://" + mi.domain_name + mi.url_path;
                    


                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-begin/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mi.date_begin))
                        mi.date_begin = default(DateTime);

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-end/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mi.date_end))
                        mi.date_end = default(DateTime);

                    if (!bool.TryParse(node.SelectSingleNode("expired/text()").Value, out mi.expired))
                        mi.expired = false;

                    //mi.

                    //:parse exact or calc                   
                    mi.Duration = mi.date_end.Subtract(mi.date_begin);

                    //if mDetail.date_begin is not defined and duration is 0 => then this is the folder which should be ignored
                    if (mi.date_begin.Equals(default(DateTime)) && mi.Duration.TotalMinutes == 0) continue;

                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }

                miList.Add(mi);
            }

            return miList.ToArray();
        }

        /// <summary>
        /// Provides information about a SCO on Connect Enterprise. The object can have any valid
        /// SCO type. See type for a list of the allowed SCO types.
        /// The response includes the account the SCO belongs to, the dates it was created and last
        /// modified, the owner, the URL that reaches it, and other data. For some types of SCOs, the
        /// response also includes information about a template from which this SCO was created.
        /// </summary>
        /// <param name="sco_id">meeting id</param>
        /// <returns><see cref="MeetingDetail"/></returns>
        public MeetingDetail GetMeetingDetail(string sco_id)
        {
            //act: "sco-info"

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("sco-info", string.Format("sco-id={0}", sco_id), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNode MeetingDetailNode = xDoc.SelectSingleNode("//sco");
            if (MeetingDetailNode == null || !MeetingDetailNode.HasChildNodes) return null;

            try
            {
                MeetingDetail mDetail = new MeetingDetail();
                mDetail.sco_id = MeetingDetailNode.Attributes["sco-id"].Value;
                mDetail.account_id = MeetingDetailNode.Attributes["account-id"].Value;
                mDetail.folder_id = MeetingDetailNode.Attributes["folder-id"].Value;

                mDetail.name = MeetingDetailNode.SelectSingleNode("name/text()").Value;
                if (MeetingDetailNode.SelectSingleNode("description/text()") != null)
                    mDetail.description = MeetingDetailNode.SelectSingleNode("description/text()").Value;

                mDetail.url_path = MeetingDetailNode.SelectSingleNode("url-path/text()").Value;
                /*
                if (!string.IsNullOrEmpty(mDetail.url_path))
                {
                    Uri u = new Uri(m_serviceURL);
                    mDetail.FullUrl = "https://" + u.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped) + mDetail.url_path;
                }
                */

                if (!DateTime.TryParseExact(MeetingDetailNode.SelectSingleNode("date-created/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_created))
                    mDetail.date_created = default(DateTime);

                if (!DateTime.TryParseExact(MeetingDetailNode.SelectSingleNode("date-modified/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_modified))
                    mDetail.date_modified = default(DateTime);

                if (MeetingDetailNode.SelectSingleNode("passing-score/text()") == null || !int.TryParse(MeetingDetailNode.SelectSingleNode("passing-score/text()").Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out mDetail.passing_score))
                    mDetail.passing_score = -1;

                if (MeetingDetailNode.SelectSingleNode("duration/text()") == null || !int.TryParse(MeetingDetailNode.SelectSingleNode("duration/text()").Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out mDetail.duration))
                    mDetail.duration = -1;

                if (!DateTime.TryParseExact(MeetingDetailNode.SelectSingleNode("date-begin/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_begin))
                    mDetail.date_begin = default(DateTime);

                if (!DateTime.TryParseExact(MeetingDetailNode.SelectSingleNode("date-end/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_end))
                    mDetail.date_end = default(DateTime);

                return mDetail;

            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }
            return null;
        }

        /// <summary>
        /// Method is intented to retrive data from AC 'Content' folder. E.g.: Quizzes
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList GetQuizzesInRoom(string sco_id, out StatusInfo iStatus)
        {
            iStatus = new StatusInfo();
            XmlDocument xDoc = _ProcessRequest("sco-contents", string.Format("sco-id={0}", sco_id), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList MeetingDetailNodes = xDoc.SelectNodes("//sco");
            if (MeetingDetailNodes == null || MeetingDetailNodes.Count < 1)
            {
                iStatus = ReportStatus(StatusCodes.no_data, StatusSubCodes.not_set, new ArgumentNullException("Node 'sco' is empty"));
                return null;
            }
            else
            {
                return MeetingDetailNodes;
            }

        }

        /// <summary>
        /// Returns a list of SCOs within another SCO. The enclosing SCO can be a folder, meeting, or
        /// curriculum.
        /// In general, the contained SCOs can be of any type—meetings, courses, curriculums, content,
        /// events, folders, trees, or links (see the list in type). However, the type of the contained SCO
        /// needs to be valid for the enclosing SCO. For example, courses are contained within
        /// curriculums, and meeting content is contained within meetings.
        /// Because folders are SCOs, the returned list includes SCOs and subfolders at the next
        /// hierarchical level, but not the contents of the subfolders. To include the subfolder contents,
        /// call sco-expanded-contents.
        /// </summary>
        /// <param name="sco_id">Room/Folder ID</param>
        /// <param name="iStatus">status response object returned</param>
        /// <returns><see cref="MeetingItem">MeetingItem</see> array</returns>
        public MeetingItem[] GetMeetingsInRoom(string sco_id, out StatusInfo iStatus)
        {
            //act: "sco-contents"

            iStatus = new StatusInfo();
            XmlDocument xDoc = _ProcessRequest("sco-contents", string.Format("sco-id={0}", sco_id), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList MeetingDetailNodes = xDoc.SelectNodes("//sco");
            if (MeetingDetailNodes == null || MeetingDetailNodes.Count < 1)
            {
                //iStatus = ReportStatus(StatusCodes.no_data, StatusSubCodes.not_set, new ArgumentNullException("Node 'sco' is empty"));
                TraceTool.TraceMessage("Node 'sco' is empty: no data available for sco-id=" + sco_id);
                return null;
            }

            List<MeetingItem> lstDetails = new List<MeetingItem>();
            foreach (XmlNode node in MeetingDetailNodes)
            {
                try
                {
                    MeetingItem mDetail = new MeetingItem();
                    mDetail.sco_id = node.Attributes["sco-id"].Value;
                    mDetail.folder_id = node.Attributes["folder-id"].Value;
                    if (!bool.TryParse(node.Attributes["is-folder"].Value, out mDetail.is_folder))
                        mDetail.is_folder = false;
                    if (node.Attributes["byte-count"] != null)
                        if (!long.TryParse(node.Attributes["byte-count"].Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out mDetail.byte_count))
                            mDetail.byte_count = -1;
                    if (node.Attributes["lang"] != null)
                        mDetail.language = node.Attributes["lang"].Value;
                    if (node.Attributes["type"] != null)
                        mDetail.type = (SCOtype)ReflectEnum(typeof(SCOtype), node.Attributes["type"].Value);

                    if (node.SelectSingleNode("name/text()") != null)
                        mDetail.meeting_name = node.SelectSingleNode("name/text()").Value;
                    if (node.SelectSingleNode("description/text()") != null)
                        mDetail.meeting_description = node.SelectSingleNode("description/text()").Value;

                    if (node.SelectSingleNode("sco-tag/text()") != null)
                        mDetail.sco_tag = node.SelectSingleNode("sco-tag/text()").Value;

                    mDetail.url_path = node.SelectSingleNode("url-path/text()").Value;
                    //
                    if (!string.IsNullOrEmpty(mDetail.url_path))
                    {
                        Uri u = new Uri(m_serviceURL);
                        mDetail.FullUrl = "https://" + u.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped) + mDetail.url_path;
                    }


                    //NOTE: if folder =>  date-begin is null
                    if (node.SelectSingleNode("date-begin/text()") != null)
                        if (!DateTime.TryParseExact(node.SelectSingleNode("date-begin/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_begin))
                            mDetail.date_begin = default(DateTime);

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-modified/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_modified))
                        mDetail.date_modified = default(DateTime);

                    if (node.SelectSingleNode("date-end/text()") != null)
                        if (!DateTime.TryParseExact(node.SelectSingleNode("date-end/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_end))
                            mDetail.date_end = default(DateTime);

                    mDetail.Duration = mDetail.date_end.Subtract(mDetail.date_begin);

                    //if mDetail.date_begin is not defined and duration is 0 => then this is the folder which should be ignored
                    if (mDetail.date_begin.Equals(default(DateTime)) && mDetail.Duration.TotalMinutes == 0) continue;

                    lstDetails.Add(mDetail);

                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }

            }

            return lstDetails.ToArray();
        }

        /// <summary>
        /// Returns a list of SCOs within another SCO. The enclosing SCO can be a folder, meeting, or
        /// curriculum.
        /// </summary>
        /// <param name="sco_id">Room/Folder ID</param>
        /// <param name="iStatus"></param>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList GetMeetingsInRoomRaw(string sco_id, out StatusInfo iStatus)
        {
            iStatus = new StatusInfo();
            XmlDocument xDoc = _ProcessRequest("sco-contents", string.Format("sco-id={0}", sco_id), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//sco");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Provides information about each event the current user has attended or is scheduled to attend.
        /// The user can be either a host or a participant in the event. The events returned are those in the
        /// user’s my-events folder.
        /// To obtain information about all events on your Enterprise Server or in your Enterprise Hosted
        /// account, call sco-shortcuts to get the sco-id of the events folder. Then, call scocontents
        /// with the sco-id to list all events.
        /// </summary>
        /// <returns><see cref="EventInfo">EventInfo array</see></returns>
        public EventInfo[] ReportMyEvents()
        {
            //act: "report-my-events"

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("report-my-events", null, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList eventNodes = xDoc.SelectNodes("//my-events/event");
            if (eventNodes == null || eventNodes.Count < 1) return null;

            List<EventInfo> miList = new List<EventInfo>();
            foreach (XmlNode node in eventNodes)
            {
                EventInfo ei = new EventInfo();

                try
                {
                    ei.sco_id = node.Attributes["sco-id"].Value;
                    ei.permission_id = (PermissionID)this.ReflectEnum(typeof(PermissionID), node.Attributes["permission-id"].Value);

                    ei.name = node.SelectSingleNode("name/text()").Value;
                    ei.DomainName = node.SelectSingleNode("domain-name/text()").Value;
                    ei.url_path = node.SelectSingleNode("url-path/text()").Value;

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-begin/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out ei.date_begin))
                        ei.date_begin = default(DateTime);

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-end/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out ei.date_end))
                        ei.date_end = default(DateTime);

                    if (!bool.TryParse(node.SelectSingleNode("expired/text()").Value, out ei.expired))
                        ei.expired = false;

                    ei.duration = ei.date_end.Subtract(ei.date_begin);

                    //if mDetail.date_begin is not defined and duration is 0 => then this is the folder which should be ignored
                    if (ei.date_begin.Equals(default(DateTime)) && ei.duration.TotalMinutes == 0) continue;

                    miList.Add(ei);
                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }
            }

            return miList.ToArray();
        }

        /// <summary>
        /// List all meetings on the server
        /// </summary>
        /// <returns>
        /// <see cref="MeetingItem">Meeting list</see>
        /// *Note: all dates are GMT
        /// </returns>
        public MeetingItem[] GetAllMeetings()
        {
            //act: "report-bulk-objects"

            StatusInfo iStatus;
            XmlDocument xDoc = _ProcessRequest("report-bulk-objects", "filter-type=meeting", out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList MeetingDetailNodes = xDoc.SelectNodes("//report-bulk-objects/row");
            if (MeetingDetailNodes == null || MeetingDetailNodes.Count < 1)
            {
                TraceTool.TraceMessage("Node 'report-bulk-objects' is empty: no data available");
                return null;
            }

            List<MeetingItem> lstDetails = new List<MeetingItem>();
            foreach (XmlNode node in MeetingDetailNodes)
            {
                try
                {
                    MeetingItem mDetail = new MeetingItem();
                    mDetail.sco_id = node.Attributes["sco-id"].Value;
                    mDetail.type = (SCOtype)ReflectEnum(typeof(SCOtype), node.Attributes["type"].Value);

                    if (node.SelectSingleNode("name/text()") != null)
                        mDetail.meeting_name = node.SelectSingleNode("name/text()").Value;

                    mDetail.url_path = node.SelectSingleNode("url/text()").Value;

                    if (!string.IsNullOrEmpty(mDetail.url_path))
                    {
                        Uri u = new Uri(m_serviceURL);
                        mDetail.FullUrl = "https://" + u.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped) + mDetail.url_path;
                    }

                    //NOTE: if folder =>  date-begin is null
                    if (node.SelectSingleNode("date-begin/text()") != null)
                        if (!DateTime.TryParseExact(node.SelectSingleNode("date-begin/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_begin))
                            mDetail.date_begin = default(DateTime);

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-modified/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_modified))
                        mDetail.date_modified = default(DateTime);

                    if (node.SelectSingleNode("date-end/text()") != null)
                        if (!DateTime.TryParseExact(node.SelectSingleNode("date-end/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out mDetail.date_end))
                            mDetail.date_end = default(DateTime);

                    mDetail.Duration = mDetail.date_end.Subtract(mDetail.date_begin);

                    //if mDetail.date_begin is not defined and duration is 0 => then this is the folder which should be ignored
                    if (mDetail.date_begin.Equals(default(DateTime)) && mDetail.Duration.TotalMinutes == 0) continue;

                    lstDetails.Add(mDetail);

                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }
            }

            return lstDetails.ToArray();
        }

        #endregion

        #region subscribe/unsubscribe

        /// <summary>
        /// Subscribes specific participant to specific course/event
        /// </summary>
        /// <param name="course_sco">course/event id</param>
        /// <param name="principal_id">principal/participant id</param>
        /// <returns><see cref="StatusInfo"/></returns>
        public StatusInfo SubscribeParticipant(string course_sco, string principal_id)
        {
            return this.PermissionsUpdate(course_sco, principal_id, PermissionID.view);
        }

        /// <summary>
        /// UnSubscribes specific participant from specific course/event
        /// </summary>
        /// <param name="course_sco">course/event id</param>
        /// <param name="principal_id">principal/participant id</param>
        /// <returns><see cref="StatusInfo"/></returns>
        public StatusInfo UnSubscribeParticipant(string course_sco, string principal_id)
        {
            return this.PermissionsUpdate(course_sco, principal_id, PermissionID.remove);
        }

        #endregion

        #region reports
        /// <summary>
        /// Returns a list of users who attended an Acrobat Connect meeting. The data is returned in row
        /// elements, one for each person who attended. If the meeting hasn’t started or had no attendees,
        /// the response contains no rows.The response does not include meeting hosts or users who were
        /// invited but did not attend
        /// </summary>
        /// <param name="sco_id">Meeting ID</param>
        /// <param name="filter_by">optional 'filter by' params</param>        
        /// <param name="iStatus">status response object returned</param>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_MeetingAttendance(string sco_id, string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-meeting-attendance", string.Format("sco-id={0}&{1}", sco_id, filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Returns bulk questions information
        /// </summary>
        /// <param name="filter_by">optional 'filter by' params</param> 
        /// <param name="iStatus">status response object returned</param>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_BulkQuestions(string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-bulk-questions", string.Format("{0}", filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Returns information about principal-to-SCO transactions on your server or in your hosted account.
        /// A transaction is an instance of one principal visiting one SCO. The SCO can be an Acrobat
        /// Connect Professional meeting, course, document, or any content on the server.
        /// Note: this call to report-bulk-consolidated-transactions, with filter-type=meeting, returns only
        /// users who logged in to the meeting as participants, not users who entered the meeting as guests.
        /// </summary>
        /// <returns><see cref="TransactionInfo">TransactionInfo</see> array</returns>
        public TransactionInfo[] Report_ConsolidatedTransactions(string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-bulk-consolidated-transactions", filter_by, out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            List<TransactionInfo> tInfoList = new List<TransactionInfo>();
            foreach (XmlNode node in transNodes)
            {
                TransactionInfo ti = new TransactionInfo();

                try
                {
                    ti.transaction_id = node.Attributes["transaction-id"].Value;
                    ti.sco_id = node.Attributes["sco-id"].Value;
                    ti.principal_id = node.Attributes["principal-id"].Value;
                    if (node.Attributes["type"] != null)
                        ti.type = (SCOtype)ReflectEnum(typeof(SCOtype), node.Attributes["type"].Value);
                    ti.score = node.Attributes["score"].Value;

                    ti.name = node.SelectSingleNode("name/text()").Value;
                    ti.url = node.SelectSingleNode("url/text()").Value;
                    ti.login = node.SelectSingleNode("login/text()").Value;
                    ti.user_name = node.SelectSingleNode("user-name/text()").Value;
                    if (node.SelectSingleNode("status/text()") != null)
                    {
                        ti.status = node.SelectSingleNode("status/text()").Value;
                        //ti.status = (StatusCodes)ReflectEnum(typeof(StatusCodes), node.SelectSingleNode("status/text()").Value);
                    }

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-created/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out ti.date_created))
                        ti.date_created = default(DateTime);

                    if (!DateTime.TryParseExact(node.SelectSingleNode("date-closed/text()").Value, @"yyyy-MM-dd\THH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AdjustToUniversal, out ti.date_closed))
                        ti.date_closed = default(DateTime);

                    tInfoList.Add(ti);

                }
                catch (Exception ex)
                {
                    TraceTool.TraceException(ex);
                }

            }

            return tInfoList.ToArray();
        }

        /// <summary>
        /// Provides information about all the interactions users have had with a certain quiz. An
        /// interaction identifies all answers one user makes to one quiz question. If a user answers the
        /// same question more than once, all answers are part of the same interaction and have the same interaction-id.
        /// This report provides information about every answer that any user has ever given to questions
        /// on a quiz. You can filter the response to make it more meaningful, using any allowed filters.
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_QuizInteractions(string sco_id, string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-quiz-interactions", string.Format("sco-id={0}&{1}", sco_id, filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Returns information about the number of users who chose a specific answer to a quiz
        /// question. The combination of one quiz question and all of one user’s answers to it is called an
        /// interaction. If the user answers the question more than once, all answers are part of the same
        /// interaction and have the same interaction-id
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_QuizQuestionAnswers(string sco_id, string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-quiz-question-answer-distribution", string.Format("sco-id={0}&{1}", sco_id, filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Returns information about the number of correct and incorrect answers to the questions on a
        /// quiz. This call can help you determine how a group responded to a quiz question overall.
        /// Because this call returns information about all the questions on a quiz, you may want to filter
        /// the results for a specific question or group of questions.
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_QuizQuestionDistribution(string sco_id, string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-quiz-question-distribution", string.Format("sco-id={0}&{1}", sco_id, filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Provides a list of answers that users have given to questions on a quiz.
        /// Without filtering, this action returns all answers from any user to any question on the quiz.
        /// However, you can filter the response for a specific user, interaction, or answer (see the filter
        /// syntax at filter-definition).
        /// An interaction is a combination of one user and one question. If the user answers the same
        /// question more than once, all answers are part of the same interaction and have the same interaction-id.
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_QuizQuestionResponse(string sco_id, string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-quiz-question-response", string.Format("sco-id={0}&{1}", sco_id, filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Provides a summary of data about a quiz, including the number of times the quiz has been
        /// taken, average, high, and low scores, and other information.
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_QuizSummary(string sco_id, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-quiz-summary", string.Format("sco-id={0}", sco_id), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

        /// <summary>
        /// Provides information about all users who have taken a quiz in a training. Use a sco-id to
        /// identify the quiz.
        /// To reduce the volume of the response, use any allowed filter or pass a type parameter to
        /// return information about just one type of SCO (courses, presentations, or meetings).
        /// </summary>
        /// <returns>Raw contents as <see cref="XmlNodeList"/></returns>
        public XmlNodeList Report_QuizTakers(string sco_id, string filter_by, out StatusInfo iStatus)
        {
            XmlDocument xDoc = _ProcessRequest("report-quiz-takers", string.Format("sco-id={0}&{1}", sco_id, filter_by), out iStatus);
            if (iStatus.Code != StatusCodes.ok || xDoc == null || !xDoc.HasChildNodes) return null;

            XmlNodeList transNodes = xDoc.SelectNodes("//row");
            if (transNodes == null || transNodes.Count < 1) return null;

            return transNodes;
        }

       


        #endregion

        #region internal routines

        private XmlDocument _ProcessRequest(string pAction, string qParams, out StatusInfo iStatus)
        {
            //Single sign on (SSO) implementation can be done via passing session info information as ""?session=" parameter to the Adobe Connect event url.
            if (m_sessionParam)
            {
                if (String.IsNullOrEmpty(qParams))
                {
                    qParams = "session=" + m_SessionInfo;
                }
                else
                {
                    qParams = String.Concat("session=", m_SessionInfo, @"&", qParams);
                }
            }
            return this._ProcessRequest(pAction, qParams, false, out iStatus);
        }

        private XmlDocument _ProcessRequest(string pAction, string qParams, bool extractSessionCookie, out StatusInfo iStatus)
        {
            iStatus = new StatusInfo();
            iStatus.Code = StatusCodes.not_set;

            if (qParams == null) qParams = string.Empty;

            HttpWebRequest HttpWReq = WebRequest.Create(m_serviceURL + string.Format(@"?action={0}&{1}", pAction, qParams)) as HttpWebRequest;
            if (HttpWReq == null) return null;

            try
            {
                if (!string.IsNullOrEmpty(this.m_proxyUrl))
                {
                    if (!string.IsNullOrEmpty(this.m_netUser) && !string.IsNullOrEmpty(this.m_netPassword))
                    {
                        HttpWReq.Proxy = new System.Net.WebProxy(this.m_proxyUrl, true);
                        HttpWReq.Proxy.Credentials = new NetworkCredential(this.m_netUser, this.m_netPassword, this.m_netDomain);
                    }

                }
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }

            //20 sec. timeout: A Domain Name System (DNS) query may take up to 15 seconds to return or time out.
            HttpWReq.Timeout = 20000 * 60;
            HttpWReq.Accept = "*/*";
            HttpWReq.KeepAlive = false;
            HttpWReq.CookieContainer = new CookieContainer();

            if ((!m_sessionParam) && (!extractSessionCookie))
            {
                if (!string.IsNullOrEmpty(m_SessionInfo) && !string.IsNullOrEmpty(m_SessionDomain))
                    HttpWReq.CookieContainer.Add(new System.Net.Cookie("BREEZESESSION", this.m_SessionInfo, "/", this.m_SessionDomain));
            }

            HttpWebResponse HttpWResp = null;

            try
            {
                //FIX: invalid SSL passing behavior
                //(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                ServicePointManager.ServerCertificateValidationCallback = delegate
                {
                    return true;
                };

                HttpWResp = HttpWReq.GetResponse() as HttpWebResponse;

                if (extractSessionCookie)
                {
                    if (HttpWResp.Cookies["BREEZESESSION"] != null)
                    {
                        this.m_SessionInfo = HttpWResp.Cookies["BREEZESESSION"].Value;
                        this.m_SessionDomain = HttpWResp.Cookies["BREEZESESSION"].Domain;
                        iStatus.SessionInfo = this.m_SessionInfo;
                    }
                }

                Stream receiveStream = HttpWResp.GetResponseStream();
                if (receiveStream == null) return null;

                XmlDocument xDoc = null;
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    string buf = readStream.ReadToEnd();
                    if (!string.IsNullOrEmpty(buf))
                    {
                        xDoc = new XmlDocument();
                        xDoc.Load(new StringReader(buf));

                        iStatus.InnerXml = xDoc.InnerXml;

                        object var = this.ReflectEnum(typeof(StatusCodes), xDoc.SelectSingleNode("//status/@code").Value);
                        if (var != null)
                            iStatus.Code = (StatusCodes)var;

                        switch (iStatus.Code)
                        {
                            case StatusCodes.invalid:
                                //there is not always an invalid child element
                                XmlNode node = xDoc.SelectSingleNode("//invalid");
                                if (node != null)
                                {
                                    iStatus.SubCode = (StatusSubCodes)this.ReflectEnum(typeof(StatusSubCodes), node.Attributes["subcode"].Value);
                                    iStatus.InvalidField = node.Attributes["field"].Value;
                                }
                                break;
                            case StatusCodes.no_access:
                                iStatus.SubCode = (StatusSubCodes)this.ReflectEnum(typeof(StatusSubCodes), xDoc.SelectSingleNode("//status/@subcode").Value);
                                break;
                            default: break;
                        }
                    }
                }

                return xDoc;
            }
            catch (Exception ex)
            {
                HttpWReq.Abort();
                TraceTool.TraceException(ex);
                iStatus.UndeliningExceptionInfo = ex;
            }
            finally
            {
                if (HttpWResp != null)
                    HttpWResp.Close();
            }

            return null;

        }

        string EnumToStr(Enum enu)
        {
            try
            {
                return Enum.GetName(enu.GetType(), enu).Replace('_', '-').ToLower();
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }

            return null;
        }

        Enum ReflectEnum(Type PrimaryType, string EnumField)
        {
            try
            {
                EnumField = EnumField.Replace('-', '_');
                return (Enum)Enum.Parse(PrimaryType, EnumField, true);
            }
            catch (Exception ex)
            {
                TraceTool.TraceException(ex);
            }

            return null;
        }

        string StructToQueryString(object pSetup, bool XmlElementAttributeOverride)
        {
            if (pSetup == null) return null;

            StringBuilder cmdParams = new StringBuilder();
            foreach (FieldInfo fi in pSetup.GetType().GetFields())
            {
                if (!fi.IsPublic) continue;

                object _fieldValue = fi.GetValue(pSetup);
                if (_fieldValue == null) continue;
                if (_fieldValue.GetType().Equals(typeof(bool))) _fieldValue = (_fieldValue.Equals(true)) ? 1 : 0;
                else
                    if (_fieldValue.GetType().Equals(typeof(DateTime)))
                    {
                        if (_fieldValue.Equals(DateTime.MinValue)) continue;
                        _fieldValue = ((DateTime)_fieldValue).ToString(@"yyyy-MM-dd\THH:mm");
                        //_fieldValue = ((DateTime)_fieldValue).ToString(@"yyyy-MM-dd\THH:mm:ss.fffzzz");
                    }
                    else
                        if (_fieldValue.GetType().Equals(typeof(TimeSpan)))
                        {
                            if (_fieldValue.Equals(TimeSpan.Zero)) continue;
                            _fieldValue = ((TimeSpan)_fieldValue).TotalMinutes;
                        }
                        else
                            if (_fieldValue.GetType().Equals(typeof(Enum)))
                            {
                                _fieldValue = EnumToStr((Enum)_fieldValue);
                            }


                string _filedName = fi.Name.Replace('_', '-').ToLower();
                //+replace fi.Name with XmlElement attribute value, if exist
                if (XmlElementAttributeOverride)
                {
                    XmlElementAttribute[] xmlAttr = fi.GetCustomAttributes(typeof(XmlElementAttribute), false) as XmlElementAttribute[];
                    if (xmlAttr != null && xmlAttr.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(xmlAttr[0].ElementName))
                            _filedName = xmlAttr[0].ElementName;
                    }
                }

                //the only place that references System.Web
                //cmdParams.AppendFormat("&{0}={1}", fi.Name.Replace('_', '-').ToLower(), System.Web.HttpUtility.UrlEncode(_fieldValue.ToString()));

                cmdParams.AppendFormat("&{0}={1}", _filedName, HttpUtilsInternal.UrlEncode(_fieldValue.ToString()));
            }
            return cmdParams.ToString();
        }


        StatusInfo ReportStatus(StatusCodes code, StatusSubCodes subCode, Exception exInfo)
        {
            StatusInfo iStatus = new StatusInfo();
            iStatus.Code = code;
            iStatus.SubCode = subCode;
            iStatus.UndeliningExceptionInfo = exInfo;
            return iStatus;
        }

        #endregion

    }
}