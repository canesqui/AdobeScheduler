/*
Copyright (c) 2007-2009 Dmitry Stroganov (DmitryStroganov.info)
Redistributions of any form must retain the above copyright notice.
 
Use of any commands included in this SDK is at your own risk. 
Dmitry Stroganov cannot be held liable for any damage through the use of these commands.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace AdobeConnectSDK
{
    /// <summary>
    /// StatusInfo structure, holds status information during API calls
    /// </summary>
    [Serializable]
    public class StatusInfo
    {
        public StatusCodes Code = StatusCodes.not_set;
        public StatusSubCodes SubCode= StatusSubCodes.not_set;

        public string InvalidField;

        //TODO: change name to UnderlyingExceptionInfo
        public Exception  UndeliningExceptionInfo;

        public string InnerXml = string.Empty;

        public string SessionInfo = string.Empty;

        /*
        public static StatusCodes GetStatusCodeMapping(string strStatusCode)
        {
            switch (strStatusCode.ToLower())
            {
                case "ok": return StatusCodes.OK;
                case "invalid": return StatusCodes.invalid;
                case "no-access": return StatusCodes.no_access;
                case "no-data": return StatusCodes.no_data;
                case "too-much-data": return StatusCodes.too_much_data;
                default: return StatusCodes.invalid;
            }
        }
        */
    }

    public enum StatusCodes
    {
        not_set,
        /// <summary>
        /// Indicates that the action has completed successfully.
        /// </summary>
        ok,
        /// <summary>
        /// Indicates that a call is invalid in some way. The invalid element provides more detail.
        /// </summary>
        invalid,
        /// <summary>
        /// Indicates that you don’t have permission to call the action. The subcode
        /// attribute provides more details.
        /// </summary>
        no_access,
        /// <summary>
        /// Indicates that there is no data available (in response to an action that
        /// would normally result in returning data). Usually indicates that there is
        /// no item with the ID you specified. To resolve the error, change the
        /// specified ID to that of an item that exists.
        /// </summary>
        no_data,
        /// <summary>
        /// Indicates that the action should have returned a single result but is
        /// actually returning multiple results. For example, if there are multiple
        /// users with the same user name and password, and you call the login
        /// action using that user name and password as parameters, the system
        /// cannot determine which user to log you in as, so it returns a too-muchdata error.
        /// </summary>
        too_much_data,
        internal_error
    }

    public enum StatusSubCodes
    {
        not_set,
        /// <summary>
        /// The customer account has expired.
        /// </summary>
        account_expired,
        /// <summary>
        /// Based on the supplied credentials, you don’t have permission to call the action.
        /// </summary>
        denied,
        /// <summary>
        /// The user is not logged in. To resolve the error, log in (using the login action) before you make the call. For more information, see login.
        /// </summary>
        no_login,
        /// <summary>
        /// The account limits have been reached or exceeded.
        /// </summary>
        no_quota,
        /// <summary>
        /// The required resource is unavailable.
        /// </summary>
        not_available,
        /// <summary>
        /// You must use SSL to call this action.
        /// </summary>
        not_secure,
        /// <summary>
        /// The account is not yet activated.
        /// </summary>
        pending_activation,
        /// <summary>
        /// The account’s license agreement has not been settled.
        /// </summary>
        pending_license,
        /// <summary>
        /// The course or tracking content has expired.
        /// </summary>
        sco_expired,
        /// <summary>
        /// The meeting or course has not started.
        /// </summary>
        sco_not_started,
        //--------------------
        /// <summary>
        /// The call attempted to add a duplicate item in a context where
        /// uniqueness is required.
        /// </summary>
        duplicate,
        /// <summary>
        /// The requested operation violates integrity rules (for example, moving
        /// a folder into itself).
        /// </summary>
        illegal_operation,
        /// <summary>
        /// The requested information does not exist.
        /// </summary>
        no_such_item,
        /// <summary>
        /// The value is outside the permitted range of values.
        /// </summary>
        range,
        /// <summary>
        /// A required parameter is missing.
        /// </summary>
        missing,
        /// <summary>
        /// A passed parameter had the wrong format.
        /// </summary>
        format
    }
}
