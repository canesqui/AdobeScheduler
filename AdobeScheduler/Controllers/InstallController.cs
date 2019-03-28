using AdobeScheduler.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace AdobeScheduler.Controllers
{
    public class InstallController : Controller
    {
        // GET: Install                
        public ActionResult Start()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["RunSetup"]) == true)
            {
                return View("View", new Setup());
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        // POST: Install/Create
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Setup(Setup setup)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    const string path = "~/Content/Images/";
                    const string cssPath = "~/Content/Site.css";
                    string pathToSave = Server.MapPath(path);
                    string pathToLoad = Server.MapPath(cssPath);
                    SetupDestinationFolder(pathToSave);
                    string fileName = "";
                    List<string> fileList = new List<string>();
                    foreach (string upload in Request.Files)
                    {
                        if (Request.Files[upload].ContentLength == 0) continue;
                        string extension = Path.GetExtension(Request.Files[upload].FileName);

                        fileName = Path.GetRandomFileName().Replace(@".", @"") + extension;
                        Request.Files[upload].SaveAs(Path.Combine(pathToSave, fileName));
                        fileList.Add(fileName);
                    }

                    using (System.IO.StreamWriter cssFile = new System.IO.StreamWriter(pathToLoad, true))
                    {
                        cssFile.WriteLine(@":root { --accent-color: " + setup.Color +";}");                        
                    }

                    //using (AdobeConnectDB _db = new AdobeConnectDB(setup.ConnectionString))
                    //{
                    //_db.Database.CreateIfNotExists();
                    //}

                    Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                    ConnectionStringsSection configSection = config.GetSection("connectionStrings") as ConnectionStringsSection;
                    if (configSection != null)
                    {
                        configSection.ConnectionStrings["DefaultConnection"].ConnectionString = setup.ConnectionString;                        
                    }

                    string createString = "" +
                        "CREATE TABLE [dbo].[Appointments]([id][int] IDENTITY(1, 1) NOT NULL,"+
                                                           "[userId] [nvarchar] (max) NULL," +
                                                           "[title] [nvarchar] (max) NULL,"+
                                                           "[url] [nvarchar] (max) NULL,"+
                                                           "[adobeUrl] [nvarchar] (max) NULL,"+
                                                           "[start] [datetime] NOT NULL,"+
                                                           "[allDay] [bit] NOT NULL,"+
                                                           "[roomSize] [int] NOT NULL,"+
                                                           "[end] [datetime] NOT NULL,"+
                                                           "[repetitionId] [nvarchar] (max) NULL,"+
                                                           "[endRepDate] [datetime] NULL,"+
	                                                       "[repetitionType] [nvarchar] (max) NOT NULL,"+
	                                                       "[Room_id] [int] NULL,"+
                                                           "CONSTRAINT[PK_Appointments2] PRIMARY KEY CLUSTERED "+
                                                           "("+
                                                            "[id] ASC"+
                                                           ")WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]"+
                                                           ") ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]" +                        
                        "";

                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(setup.ConnectionString);

                    //string server = builder.DataSource;
                    string userDatabase = builder.InitialCatalog;
                    //string server = builder.DataSource;
                    builder.InitialCatalog = "master";

                    using (var conn = new SqlConnection(builder.ConnectionString))
                    {                        
                        conn.Open();
                        var command = conn.CreateCommand();
                        command.CommandText = "CREATE DATABASE "+userDatabase;
                        command.ExecuteNonQuery();
                    }

                    SqlConnection connection = new SqlConnection(setup.ConnectionString);
                    SqlCommand create = new SqlCommand(createString, connection);
                    connection.Open();
                    create.ExecuteNonQuery();
                    connection.Close();

                    /*
                    <add key="NetDomain" value="turner.southern.edu" />
                    <add key="ServiceURL" value="http://turner.southern.edu/api/xml/" />
                    <add key="UseSessionParam" value="true" />
                    <add key="AdobeConnectLicensesAvailable" value="70" />    
                    <add key="BaseLineTimeZone" value="Eastern Standard Time"/>    
                    <add key="logo" value="/Content/images/logo-login.jpg"/>
                    <add key="application_name" value="Online Campus"/>
                    */



                    AppSettingsSection appSettingsSection = config.GetSection("appSettings") as AppSettingsSection;
                    if (appSettingsSection != null)
                    {
                        appSettingsSection.Settings.Remove("NetDomain");
                        appSettingsSection.Settings.Remove("ServiceURL");
                        appSettingsSection.Settings.Remove("AdobeConnectLicensesAvailable");
                        appSettingsSection.Settings.Remove("BaseLineTimeZone");
                        appSettingsSection.Settings.Remove("BigLogo");
                        appSettingsSection.Settings.Remove("SmallLogo");
                        appSettingsSection.Settings.Remove("BackgroundImage");
                        appSettingsSection.Settings.Remove("ApplicationName");
                        appSettingsSection.Settings.Remove("RunSetup");

                        appSettingsSection.Settings.Add("NetDomain", new Uri(setup.AdobeConnectUrl).Host);
                        appSettingsSection.Settings.Add("ServiceURL", setup.AdobeConnectUrl);
                        appSettingsSection.Settings.Add("AdobeConnectLicensesAvailable", setup.AvailableLicenses.ToString());
                        appSettingsSection.Settings.Add("BaseLineTimeZone", setup.BaseLineTimeZone);
                        appSettingsSection.Settings.Add("BigLogo", "/Content/Images/" + fileList[0]);
                        appSettingsSection.Settings.Add("SmallLogo", "/Content/Images/" + fileList[1]);
                        appSettingsSection.Settings.Add("BackgroundImage", "/Content/Images/" + fileList[2]);
                        appSettingsSection.Settings.Add("ApplicationName", setup.ApplicationName);                        
                        appSettingsSection.Settings.Add("RunSetup", "false");
                    }

                    config.Save();
                    ConfigurationManager.RefreshSection("connectionStrings");
                    return RedirectToAction("login", "Auth");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("url", ex.Message);
                    return View("View", setup);
                }
            }
            else
            {
                return View();
            }

        }


        private void SetupDestinationFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
