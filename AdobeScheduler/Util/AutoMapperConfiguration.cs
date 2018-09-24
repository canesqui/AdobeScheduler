using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdobeScheduler.Util
{
    public static class AutoMapperConfiguration
    {
        public static void Configure(IMapperConfigurationExpression config)
        {
            config.CreateMap<AdobeConnectSDK.Model.MeetingItem, Models.MeetingItem>();
            config.CreateMap<AdobeConnectSDK.Model.UserInfo, Models.UserInfo>();            
        }
    }
}