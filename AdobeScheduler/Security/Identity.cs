using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using AdobeConnectSDK;
using System.Runtime.InteropServices;

namespace AdobeScheduler.Security
{
 
    public class Identity : IIdentity
    {

        public Identity(int id, string email, string roles, string adobeConnectSDKsessionId)
        {
            this.ID = id;
            this.Name = email;
            this.Email = email;
            this.Roles = roles;
            this.AdobeConnectSDKsessionId = adobeConnectSDKsessionId;
        }


        
        public Identity(string name, string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException();

            string[] values = data.Split('|');
            if (values.Length != 3)
                throw new ArgumentException();

            this.Name = name;
            this.ID = Convert.ToInt32(values[0]);
            Roles = values[2];
        }


        public string AuthenticationType
        {
            get { return "AdobeAuth"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string GetUserData()
        {
            return string.Format("{0}|{1}|{2}", ID, Roles, AdobeConnectSDKsessionId);
        }


        public string Name { get; private set; }
        public string Email { get; private set; }
        public int ID { get; private set; }
        public string Roles { get; private set; }
        public string AdobeConnectSDKsessionId { get; set; }
    }
}