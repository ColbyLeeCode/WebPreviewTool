using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebPreviewTool.Models;
using Microsoft.AspNet.Identity;

namespace WebPreviewTool.Controllers
{

    public class UploadController : Controller
    {
        WebSnap ws;

        // GET: Upload  
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            try
            {                                     
                ws = new WebSnap(User.Identity.GetUserId());

                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    file.SaveAs(_path);

                    //Parse the bookmarks file into a list of URLS 
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(_path);

                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        HtmlAttribute att = link.Attributes["href"];
                        ws.urls.Add(att.Value);
                    }
                    //start snapper on new thread
                    var t = new Thread(Snapper);
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();  
                }
                ViewBag.Message = "File Uploaded Successfully!!";

                return View();
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;
                return View();
            }
        }


        //Attempts to take a picture of loaded url and store entry in in database.
        private void Snapper()
        {
                ws.getSnap(ws.urls, 800, 600);
        }
    }
}