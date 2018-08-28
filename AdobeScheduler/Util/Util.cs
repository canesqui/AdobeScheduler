using AdobeScheduler.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdobeScheduler.Util
{
    public static class Utilities
    {
        public static TDestination Adapter<TDestination, TSource>(TSource source)
            where TSource : class
            where TDestination : class
        {
            return Mapper.Map<TDestination>(source);
        }                                                        
    }
}