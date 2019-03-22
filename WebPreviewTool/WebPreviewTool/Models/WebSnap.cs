using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WebPreviewTool.Models
{
    public class WebSnap
    {
        public List<string> urls = new List<string>();
        private int count = 0;//ticker for document.complete events called
        private bool incomplete = true;
        private readonly string requestedBy;
        private string currentUrl;

        public WebSnap(string requestedBy)
        {
            this.requestedBy = requestedBy;
        }

        public void getSnap(string url, int width = 1920, int height = 1080)
        {
            //Set IE Emulator to newest IE version installed
            forceBrowserVersion();
            using (WebBrowser browser = new WebBrowser())
            {
                currentUrl = url;
                browser.ScriptErrorsSuppressed = true;
                browser.Width = width;
                browser.Height = height;
                browser.ScrollBarsEnabled = false;
                // This will be called when the page finishes loading
                browser.DocumentCompleted += this.OnDocumentCompleted;
                browser.Navigate(url);
                // This prevents the application from exiting until
                // Application.Exit is called
                Application.Run();
            }
        }

        public void getSnap(List<string> urls, int width = 1920, int height = 1080)
        {
            //Set IE Emulator to newest IE version installed
            forceBrowserVersion();
            foreach (string url in urls)
            {
                this.count = 0;
                using (WebBrowser browser = new WebBrowser())
                {
                    currentUrl = url;
                    browser.ScriptErrorsSuppressed = true;
                    browser.Width = width;
                    browser.Height = height;
                    browser.ScrollBarsEnabled = false;           
                    // This will be called when the page finishes loading
                    browser.DocumentCompleted += this.OnDocumentCompleted;
                    browser.Navigate(url);                
                    // Wait for borwser document complete event
                    while (incomplete)
                    {
                        Application.DoEvents();
                    }
                    incomplete = true;
                    count = 0;
                }
            }
        }

        private void forceBrowserVersion()
        {
            int BrowserVer, RegVal;

            // get the installed IE version
            using (WebBrowser Wb = new WebBrowser())
                BrowserVer = Wb.Version.Major;
            // set the appropriate IE version
            if (BrowserVer >= 11)
                RegVal = 11001;
            else if (BrowserVer == 10)
                RegVal = 10001;
            else if (BrowserVer == 9)
                RegVal = 9999;
            else if (BrowserVer == 8)
                RegVal = 8888;
            else
                RegVal = 7000;

            // set the actual key
            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree))
                if (Key.GetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe") == null)
                    Key.SetValue(System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe", RegVal, RegistryValueKind.DWord);
        }

        public void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Now that the page is loaded, save it as a png

            //If for some reason the pages document complete function was called more than once just take one picture
            if (this.count < 1)
            {
                WebBrowser browser = (WebBrowser)sender;
                using (Graphics graphics = browser.CreateGraphics())
                using (Bitmap bitmap = new Bitmap(browser.Width, browser.Height, graphics))
                {
                    Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    browser.DrawToBitmap(bitmap, bounds);
                    Guid guid = Guid.NewGuid();
                    string fname = Convert.ToString(guid) + ".png";
                    string fLoc = "/Snaps/" + fname;
                    //if bookmark added to server, save picture of the site 
                    if(addToBookmarks(fname, fLoc, currentUrl))
                        bitmap.Save(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Snaps/"), fname), ImageFormat.Png);
                    this.count++;
                    this.incomplete = false;
                }
                Console.WriteLine("Capture completed.");
            }

            //exit the function without taking snap
            else
                return;
        }

        private bool addToBookmarks(string fname, string floc, string url)
        {
            //DB connection string
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=aspnet-WebPreviewTool-20180929034718;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";


            //Check is url is already used as a bookmark
            string dbid = "";
            using (SqlConnection myConnection = new SqlConnection(connectionString))
            {
                myConnection.Open();
                
                string commandText = "SELECT id FROM bookmarks WHERE siteUrl = @url";
                SqlCommand cmd = new SqlCommand(commandText, myConnection);
                SqlParameter dburl = cmd.Parameters.Add("@url", SqlDbType.VarChar);
                dburl.Value = url;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dbid = String.Format("{0}", reader["id"]); 
                    }
                }
                myConnection.Close();
            }
            if (dbid.Length < 1)
            {
                dbid = Convert.ToString(Guid.NewGuid());
            }
            
            using (SqlConnection myConnection = new SqlConnection(connectionString))
            {
                myConnection.Open();
                string query = "INSERT INTO bookmarks(id, siteUrl, fileLoc)";
                query += "VALUES (@id, @siteUrl, @fileLoc)";
                //ADD BOOKMARK
                using (SqlCommand command = new SqlCommand(query, myConnection))
                {
                    command.Parameters.AddWithValue("@id", dbid);
                    command.Parameters.AddWithValue("@siteUrl", currentUrl);
                    command.Parameters.AddWithValue("@fileLoc", floc);
                    try
                    {
                        command.ExecuteNonQuery();
                       
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);                       
                    }
                    myConnection.Close();
                }          
            }

            using (SqlConnection myConnection = new SqlConnection(connectionString))
            {
                myConnection.Open();
                string newquery = "INSERT INTO userbookmarks(userId, bookId)";
                newquery += "VALUES (@userId, @bookId)";

                using (SqlCommand command2 = new SqlCommand(newquery, myConnection))
                {
                    try
                    {
                        //ASSOCIATE BOOKMARK WITH USER
                        command2.Parameters.Add("@userId", SqlDbType.NVarChar).Value = requestedBy;
                        command2.Parameters.Add("@bookId", SqlDbType.VarChar).Value = dbid;

                        try
                        {
                            command2.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e.Message);
                        return false;
                    }

                }
                myConnection.Close();
            }
            return true;
        }
    }

}
