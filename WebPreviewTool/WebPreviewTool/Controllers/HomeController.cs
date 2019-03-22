using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebPreviewTool.Models;
using WebPreviewTool.ViewModel;

namespace WebPreviewTool.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=aspnet-WebPreviewTool-20180929034718;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public ActionResult Index()
        {

            List<string> tags = new List<string>();
            List<string> collections = new List<string>();

            if (Request.IsAuthenticated)
            {
                //build user model
                ToolUser usr = new ToolUser(User.Identity.GetUserId(), User.Identity.Name, new List<Bookmark>());

                //get bookmarks
                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    conn.Open();
                    //get all bookmarks from db of user
                    string commandText = "SELECT id, siteUrl, fileLoc FROM bookmarks b INNER JOIN userbookmarks ub ON ub.bookId = b.id WHERE ub.userId = @name";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    SqlParameter name = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    name.Value = usr.id;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usr.bookmarks.Add(new Bookmark(String.Format("{0}", reader["siteUrl"]), Convert.ToString(reader["fileLoc"]), Convert.ToString(reader["id"])));
                        }
                    }
                    conn.Close();
                }

                //Get all the collections associated with this user from the server
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string commandText = "select label from collectionMarks where userId = @user";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);

                    user.Value = User.Identity.GetUserId();


                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string label = String.Format("{0}", reader["label"]);
                            if (!collections.Contains(label))
                                collections.Add(label);
                        }
                    }
                }

                //Get all the tags associated with this user
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string commandText = "select tag from Tags t inner join BookmarkTags bmt on t.id = bmt.tagId where bmt.userId = @user";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);
                    user.Value = User.Identity.GetUserId();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tag = String.Format("{0}", reader["tag"]);
                            if (!tags.Contains(tag))
                                tags.Add(tag);

                        }
                    }
                }
                //display all bookmarks/bookmarks home
                return View("MarkView", new MarkDisplayVM(usr.bookmarks, collections, tags));
            }
            return View();
        }

        public ActionResult SortedCollections(string collection)
        {
            return View("MarkView", new MarkDisplayVM(GetBookmarks(collection: collection), GetCollections(), GetTags()));
        }

        public ActionResult SortedTags(string tag)
        {
            return View("MarkView", new MarkDisplayVM(GetBookmarks(tag: tag), GetCollections(), GetTags()));
        }

        public List<Bookmark> GetBookmarks(string collection = null, string tag = null)
        {
            //build user model
            ToolUser usr = new ToolUser(User.Identity.GetUserId(), User.Identity.Name, new List<Bookmark>());

            //get bookmarks
            List<Bookmark> bookmarks = new List<Bookmark>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = null;
                string commandText;

                //get all bookmarks from db of user
                if (collection == null && tag == null)
                {
                    commandText = "SELECT id, siteUrl, fileLoc FROM bookmarks b INNER JOIN userbookmarks ub ON ub.bookId = b.id WHERE ub.userId = @name";
                    cmd = new SqlCommand(commandText, conn);
                    SqlParameter name = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    name.Value = usr.id;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookmarks.Add(new Bookmark(String.Format("{0}", reader["siteUrl"]), Convert.ToString(reader["fileLoc"]), Convert.ToString(reader["id"])));
                        }
                    }
                }
                else if (collection != null && tag == null)
                {
                    commandText = "SELECT id, siteUrl, fileLoc FROM bookmarks b "+
                                  "INNER JOIN userbookmarks ub ON ub.bookId = b.id "+
                                  "INNER JOIN collectionMarks cm ON ub.bookId = cm.markId "+
                                  "WHERE ub.userId = @name AND cm.label = @collection";
                    cmd = new SqlCommand(commandText, conn);
                    SqlParameter name = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    SqlParameter collect = cmd.Parameters.Add("@collection", SqlDbType.VarChar);
                    name.Value = usr.id;
                    collect.Value = collection;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookmarks.Add(new Bookmark(String.Format("{0}", reader["siteUrl"]), Convert.ToString(reader["fileLoc"]), Convert.ToString(reader["id"])));
                        }
                    }
                }
                else if (collection == null && tag != null)
                {
                    commandText = "SELECT b.id, siteUrl, fileLoc FROM bookmarks b " +
                                  "INNER JOIN userbookmarks ub ON ub.bookId = b.id " +
                                  "INNER JOIN BookmarkTags bt ON ub.bookId = bt.markId " +
                                  "INNER JOIN Tags t ON t.id = bt.tagId " +
                                  "WHERE ub.userId = @name AND t.tag = @tag";
                    cmd = new SqlCommand(commandText, conn);
                    SqlParameter name = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                    SqlParameter tagLabel = cmd.Parameters.Add("@tag", SqlDbType.VarChar);
                    name.Value = usr.id;
                    tagLabel.Value = tag;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bookmarks.Add(new Bookmark(String.Format("{0}", reader["siteUrl"]), Convert.ToString(reader["fileLoc"]), Convert.ToString(reader["id"])));
                        }
                    }
                }


                //get all bookmarks in collection 


                conn.Close();
            }
            return bookmarks;
        }

        public List<string> GetCollections()
        {
            List<string> collections = new List<string>();
            //Get all the collections associated with this user from the server
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string commandText = "select label from collectionMarks where userId = @user";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);

                user.Value = User.Identity.GetUserId();


                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string label = String.Format("{0}", reader["label"]);
                        if (!collections.Contains(label))
                            collections.Add(label);
                    }
                }
            }
            return collections;
        }

        public List<string> GetTags()
        {
            List<string> tags = new List<string>();
            //Get all the tags associated with this user
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string commandText = "select tag from Tags t inner join BookmarkTags bmt on t.id = bmt.tagId where bmt.userId = @user";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);
                user.Value = User.Identity.GetUserId();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tag = String.Format("{0}", reader["tag"]);
                        if (!tags.Contains(tag))
                            tags.Add(tag);

                    }
                }
            }

            return tags;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        void SayHello()
        {
            Console.WriteLine("Hello");
        }


        //If collection type exists add bookmark to collection. 
        [HttpPost]
        public ActionResult CollectionsAdd(string id, string collection)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine(User.Identity.ToString());
                string userId = User.Identity.GetUserId();

                //Add collection type to table
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string commandText = "INSERT INTO singleCollections (name)values(@collection);";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    {
                        cmd.Parameters.AddWithValue("@collection", collection);

                        conn.Open();

                        //will throw error if collection already exists
                        try
                        {
                            int result = cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            int pass = 0;
                        }
                    }
                }

                //Get ID of collection
                string collectionId = "";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string commandText = "select id from singleCollections where name = @collection";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    SqlParameter mark = cmd.Parameters.Add("@collection", SqlDbType.VarChar);

                    mark.Value = collection;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            collectionId = String.Format("{0}", reader["id"]);
                        }
                    }
                }


                //Relate collection, bookmark and user
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string commandText = "INSERT INTO collectionMarks (markId, userId, label)values(@markId, @userId, @label)";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    {
                        cmd.Parameters.AddWithValue("@markId", id);
                        cmd.Parameters.AddWithValue("@userId", User.Identity.GetUserId());                      
                        cmd.Parameters.AddWithValue("@label", collection);

                        conn.Open();

                        //will throw error if collection already exists
                       
                            int result = cmd.ExecuteNonQuery();                   
                    }
                }
            }
            return RedirectToAction("SingleMark", "Home", new { id });
        }

        [HttpPost]
        public ActionResult CollectionsDelete(string id, string collection)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string commandText = "DELETE FROM collectionMarks where markId = @markId AND userId = @userId AND label = @label;";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                {
                    cmd.Parameters.AddWithValue("@markId", id);
                    cmd.Parameters.AddWithValue("@userId", User.Identity.GetUserId());
                    cmd.Parameters.AddWithValue("@label", collection);

                    conn.Open();

                    //will throw error if collection already exists

                    int result = cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("SingleMark", "Home", new { id });
        }

        [HttpPost]
        public ActionResult TagsDelete(string id, string tag)
        {
            //get id of tag
            string tagId = "";
            //Get ID of Tag
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string commandText = "select id from Tags where tag = @tag";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                cmd.Parameters.AddWithValue("@tag", tag);


                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tagId = String.Format("{0}", reader["id"]);
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string commandText = "DELETE FROM BookmarkTags where markId = @markId AND userId = @userId AND tagId = @tag;";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                {
                    cmd.Parameters.AddWithValue("@markId", id);
                    cmd.Parameters.AddWithValue("@userId", User.Identity.GetUserId());
                    cmd.Parameters.AddWithValue("@tag", tagId);

                    conn.Open();

                    //will throw error if collection already exists

                    int result = cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("SingleMark", "Home", new { id });
        }


        [HttpPost]
        public ActionResult TagsAdd(string id, string tag)
        {
            if (ModelState.IsValid)
            {
                //Add tag type to table
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string commandText = "INSERT INTO Tags (tag)values(@tag);";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    {
                        cmd.Parameters.AddWithValue("@tag", tag);

                        conn.Open();

                        //will throw error if collection already exists
                        try
                        {
                            int result = cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            int pass = 0;
                        }
                    }
                }

                string tagId = "";
                //Get ID of Tag
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string commandText = "select id from Tags where tag = @tag";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    cmd.Parameters.AddWithValue("@tag", tag);


                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tagId = String.Format("{0}", reader["id"]);
                        }
                    }
                }

                //Add collection type to table
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string commandText = "INSERT INTO BookmarkTags (markId, tagId, userId)values(@bmId, @tagId, @userId);";
                    SqlCommand cmd = new SqlCommand(commandText, conn);
                    {
                        cmd.Parameters.AddWithValue("@bmId", id);
                        cmd.Parameters.AddWithValue("@tagId", tagId);
                        cmd.Parameters.AddWithValue("@userId", User.Identity.GetUserId());

                        conn.Open();

                        //will throw error if collection already exists
                        try
                        {
                            int result = cmd.ExecuteNonQuery();
                        }
                        catch
                        {
                            int pass = 0;
                        }
                    }
                }

                
            }
                Console.WriteLine("Adding " + id + "to Tags");

            return RedirectToAction("SingleMark", "Home", new { id });
        }



        [HttpGet]
       public ActionResult SingleMark(string id)
        {
            SingleMarkVM smvm = new SingleMarkVM();
            Bookmark bookmark = new Bookmark();

            //Build bookmark object from db using the bookmark id passed in, add bookmark to singlemark view model
            bookmark.id = id;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string commandText = "select siteUrl, fileLoc from bookmarks where id = @mark";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter mark = cmd.Parameters.Add("@mark", SqlDbType.VarChar);

                mark.Value = bookmark.id;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookmark.url = String.Format("{0}", reader["siteUrl"]);
                        bookmark.imgLoc = String.Format("{0}", reader["fileLoc"]);
                    }
                }
            }

            //Get all the collections associated with this mark from the server
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string commandText = "select label from collectionMarks where userId = @user AND markId = @mark";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter mark = cmd.Parameters.Add("@mark", SqlDbType.VarChar);
                SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);

                user.Value = User.Identity.GetUserId();
                mark.Value = bookmark.id;



                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookmark.collections.Add(String.Format("{0}", reader["label"]));
                    }
                }
            }

            //Get all the tags associated with this mark from the server
            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                conn.Open();
                //get all bookmarks from db of user
                string commandText = "select tag from Tags t inner join BookmarkTags bm on t.Id = bm.tagId inner join bookmarks b on bm.markId = b.id WHERE bm.userId = @name AND bm.markId = @mark";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter name = cmd.Parameters.Add("@name", SqlDbType.NVarChar);
                SqlParameter mark = cmd.Parameters.Add("@mark", SqlDbType.NVarChar);
                name.Value = User.Identity.GetUserId();
                mark.Value = bookmark.id;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookmark.tags.Add(String.Format("{0}", reader["tag"]));
                    }
                }
                conn.Close();
            }

            //Get all the collections associated with this user from the server
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string commandText = "select label from collectionMarks where userId = @user";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);

                user.Value = User.Identity.GetUserId();


                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string label = String.Format("{0}", reader["label"]);
                        if (!smvm.Collections.Contains(label))
                            smvm.Collections.Add(label);
                    }
                }
            }

            //Get all the tags associated with this user
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string commandText = "select tag from Tags t inner join BookmarkTags bmt on t.id = bmt.tagId where bmt.userId = @user";
                SqlCommand cmd = new SqlCommand(commandText, conn);
                SqlParameter user = cmd.Parameters.Add("@user", SqlDbType.VarChar);
                user.Value = User.Identity.GetUserId();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tag = String.Format("{0}", reader["tag"]);
                        if (!smvm.Tags.Contains(tag))
                            smvm.Tags.Add(tag);
                        
                    }
                }
            }

            smvm.Bm = bookmark;

            return View(smvm);
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Clicked button");

        }
    }
   
}