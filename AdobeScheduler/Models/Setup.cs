using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdobeScheduler.Models
{
    public class Setup
    {
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name= "Application Name")]
        public string ApplicationName { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "SQL Server Connection String")]
        public string ConnectionString { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Adobe Connect API Url")]
        [Url]
        public string AdobeConnectUrl { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Available licenses")]
        [Range(1, int.MaxValue)]
        public int AvailableLicenses { get; set; }
        
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Baseline time zone")]
        public string BaseLineTimeZone { get; set; }
        
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Accent color")]
        public string Color { get; set; }
        
        public Setup()
        {            
            ConnectionString = @"Data Source=server;Initial Catalog=Scheduler;user=user;password=password;";
        }
    }
}