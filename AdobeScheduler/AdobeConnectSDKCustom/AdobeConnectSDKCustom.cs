namespace AdobeConnectSDK.Extensions
{
   using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using AdobeConnectSDK.Common;
    using AdobeConnectSDK.Interfaces;
    using AdobeConnectSDK.Model;

    public static class SAUOC
    {
        #region SAUOC : Custom Methods

        /// <summary>
        /// Returns true if user is Admin
        /// </summary>
        /// <param name="acl_id">acl_id of the current user</param>
        /// <returns><see cref="bool"/> bool : user us admin ? true : false</returns>
        public static bool IsAdmin(AdobeConnectXmlAPI adobeConnectXmlApi, string acl_id)
        {
            ApiStatus apiStatus = adobeConnectXmlApi.ProcessApiRequest("permissions-info", string.Format("acl-id={0}&filter-type=live-admins", acl_id));

            var resultStatus = Helpers.WrapBaseStatusInfo<EnumerableResultStatus<XElement>>(apiStatus);

            if (apiStatus.Code == StatusCodes.OK || apiStatus.ResultDocument != null) return true;            
            return false;
        }

        /// <summary>
        /// Returns the list of all rooms
        /// </summary>
        /// <remarks This function facilates the need to return the list of all 
        /// urls/rooms for admin view
        /// <returns><see cref="List<List<bool>>"/>List of List of strings {}</returns>
        public static EnumerableResultStatus<List<List<string>>> GetSharedList(AdobeConnectXmlAPI adobeConnectXmlApi)
        {
            //declare status object to determine if valid
            ApiStatus apiStatus = adobeConnectXmlApi.ProcessApiRequest("sco-expanded-contents", "sco-id=11002");

            //declare results list
            var resultStatus = Helpers.WrapBaseStatusInfo<EnumerableResultStatus<List<List<String>>>>(apiStatus);

            
            //create xDoc based off of processed request (using the meetings sco-id [11002]), terminate if not valid data
            
            if (apiStatus.Code != StatusCodes.OK || resultStatus == null || resultStatus.Result.Count() == 0) return null;

            /*
            foreach (XmlNode node in xDoc.ChildNodes[1].ChildNodes[1].ChildNodes)
            {
                //add expanded sco-nodes childrens name and url-path attributes to the results list
                if (node.ChildNodes[0].InnerText.IndexOf("/") == -1 && node.Attributes["content-source-sco-icon"].Value == "3")
                {
                    results.Add(new List<string> { node.ChildNodes[0].InnerText, node.ChildNodes[1].InnerText });
                }
            }
            */
            //return the list
            return resultStatus;
            
        }

        #endregion 
    }
}