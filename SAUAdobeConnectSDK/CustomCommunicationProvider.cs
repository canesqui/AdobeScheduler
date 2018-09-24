using AdobeConnectSDK.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SAUAdobeConnectSDK
{
    public class CustomCommunicationProvider: HttpCommunicationProvider
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CustomCommunicationProvider(string sessionInfo, string sessionDomain) : base() {
            this.sessionDomain = sessionDomain;
            this.sessionInfo = sessionInfo;
        }

        private readonly string sessionInfo;
        private readonly string sessionDomain;

        public override HttpWebRequest SetHttpConfiguration(HttpWebRequest webRequest)
        {
            base.SetHttpConfiguration(webRequest);
            webRequest.CookieContainer.Add(new Cookie("BREEZESESSION", this.sessionInfo, "/", this.sessionDomain));
            Log.Debug(webRequest.RequestUri.ToString());
            return webRequest;
        }        
    }
}
