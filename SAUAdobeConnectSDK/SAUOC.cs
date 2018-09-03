using AdobeConnectSDK;
using AdobeConnectSDK.Common;
using AdobeConnectSDK.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SAUAdobeConnectSDK
{
    public class SAUOC
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
        public static List<List<String>> GetSharedList(AdobeConnectXmlAPI adobeConnectXmlApi)
        {
            //declare status object to determine if valid
            ApiStatus apiStatus = adobeConnectXmlApi.ProcessApiRequest("sco-expanded-contents", "sco-id=11002");

            var root = apiStatus.ResultDocument.Root.Descendants("expanded-scos");

            var queryResult = from t in root.Descendants("sco") where (string)t.Attribute("content-source-sco-icon") == "3" select new { name = t.Element("name").Value, ulrPath = t.Element("url-path").Value };
                       
            List<List<String>> result = new List<List<String>>();

            foreach (var item in queryResult)
            {
                result.Add(new List<String> { item.name.ToString(), item.ulrPath.ToString()});
            }

            return result;
        }

        #endregion 
    }

}
