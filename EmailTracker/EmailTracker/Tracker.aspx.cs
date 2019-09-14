using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace EmailTracker
{
    public partial class Tracker : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // check if-modified-since header to determine if receiver has requested the image in last 24 hours
            if (CheckIfRequested(this.Context.Request))
            {
                //receiver had already requested the image, hence send back a not modified result
                Response.StatusCode = 304;
                Response.SuppressContent = true;
            }
            else
            {
                string eid = ""; //EID is ticks + "_" + GUID
                string ipaddr = "";
                if (!string.IsNullOrEmpty(Request.QueryString["eid"]))
                {
                    eid = Request.QueryString["eid"];
                    //ipaddr = Request.UserHostAddress;
                    string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (!string.IsNullOrEmpty(ipAddress))
                    {
                        string[] addresses = ipAddress.Split(',');
                        if (addresses.Length != 0)
                        {
                            ipaddr = addresses[0];
                        }
                    }
                    else
                    {
                        ipaddr = Request.ServerVariables["REMOTE_ADDR"];
                    }

                    //The email with email id has been opened, so log that in database
                    using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("EmailOpened", connection))
                        {
                            try
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@ipaddr", ipaddr);
                                command.Parameters.AddWithValue("@ueid", eid);
                                if (connection.State == ConnectionState.Closed)
                                    connection.Open();
                                command.ExecuteNonQuery();
                                connection.Close();
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine("<<<<<<<<<<< ERROR in SqlCommand EmailOpened >>>>>>>>>>>>>>>>>>>>");
                                System.Diagnostics.Debug.WriteLine("SqlCommand EmailOpened Exception: {0}", ex.Message);
                                //Only Create a log file if it does not exit
                                string logPath = AppDomain.CurrentDomain.BaseDirectory + "EmailTracker-ErrorLog_" + DateTime.Now.ToString("yyyy-MM-dd_HH") + ".txt"; 
                                System.Diagnostics.Debug.WriteLine("Log Path: " + logPath);
                                if (!System.IO.File.Exists(logPath))
                                {
                                    var logFile = System.IO.File.Create(logPath);
                                    logFile.Close();
                                }
                                StreamWriter streamWriter = new StreamWriter(path: logPath, append: true);
                                streamWriter.WriteLine(DateTime.Now);
                                streamWriter.WriteLine("Email EID to Insert: " + eid);
                                streamWriter.WriteLine("SqlCommand EmailOpened Exception: {0}", ex.Message);
                                streamWriter.WriteLine(ex.StackTrace);
                                streamWriter.Flush();
                                streamWriter.Close();
                            }
                        }
                    }
                }

                //Send the single pixel gif image as response
                byte[] imgbytes = Convert.FromBase64String("R0lGODlhAQABAIAAANvf7wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==");
                Response.ContentType = "image/gif";
                Response.AppendHeader("Content-Length", imgbytes.Length.ToString());
                Response.Cache.SetLastModified(DateTime.Now);
                Response.Cache.SetCacheability(HttpCacheability.Public);
                Response.BinaryWrite(imgbytes);
            }
        }

        private bool CheckIfRequested(HttpRequest req)
        {
            // check if-modified-since header to check if receiver has already requested the image in last 24 hours
            return req.Headers["If-Modified-Since"] == null ? false : DateTime.Parse(req.Headers["If-Modified-Since"]).AddHours(24) >= DateTime.Now;
        }

    }
}