using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Tatvasoft_Project.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using System.Dynamic;

namespace Tatvasoft_Project.Controllers
{
    public class Service_ProviderController : Controller
    {
        private HelperlandContext _helperlandcontext;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SP_Dashboard()
        {
            _helperlandcontext = new HelperlandContext();
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().ZipCode;

            var targetuserid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.ZipCode == userid && x.Status == null).ToList();
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.ServiceRequest temp in model_to_pass)
            {

                if (temp.ServiceRequestId >= 4)
                {
                    var spid = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList();

                    var blocked_count = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.UserId == spid.FirstOrDefault().UserId
                    && x.TargetUserId == targetuserid && x.IsBlocked == true).ToList();

                    if (blocked_count.Count == 0)
                    {

                        var duration = (temp.ServiceHours).ToString();
                        if (duration.Length <= 2)
                        {
                            duration += ":00";
                        }
                        else duration = Math.Round(temp.ServiceHours, 2).ToString() + '0';
                        var end_dur = Math.Round(double.Parse((temp.ServiceHours + temp.ExtraHours).ToString()), 2).ToString();
                        if (end_dur.ToString().Length <= 2)
                        {
                            duration = duration + "-" + end_dur.ToString() + ":00";
                        }
                        else duration = (duration + "-" + end_dur.ToString() + '0').Replace('.', ':');

                        int? spid3 = 0;
                        var name = "";



                        //var rating = 0;
                        if (spid.Count > 0)
                        {
                            spid3 = (spid.FirstOrDefault().UserId);
                            var fname = spid.FirstOrDefault().FirstName;
                            var lname = spid.FirstOrDefault().LastName;
                            name = fname + " " + lname;


                        }
                        item.Add(new Models.Book_now_Table
                        {
                            SP_ID = spid3,
                            SP_Name = name,
                            ID = temp.ServiceRequestId,
                            Booking_date = (temp.ServiceStartDate).Date,
                            Booking_time = (temp.ExtraHours).ToString(),
                            Discounted_cost = float.Parse((temp.SubTotal).ToString()),
                            Booking_duration = duration,
                            Suggestion = temp.Comments,
                            Status = temp.Status
                        });
                    }
                }
            }
            int i = 0;
            foreach (Models.ServiceRequest temp2 in model_to_pass)
            {
                if (temp2.ServiceRequestId >= 4)
                {
                    var spid = _helperlandcontext.Users.Where(x => x.UserId == temp2.UserId).ToList();

                    var blocked_count = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.UserId == spid.FirstOrDefault().UserId
                    && x.TargetUserId == targetuserid && x.IsBlocked == true).ToList();

                    if (blocked_count.Count == 0)
                    {



                        var address_obj = _helperlandcontext.ServiceRequestAddresses.Where(x => x.ServiceRequestId == temp2.ServiceRequestId).ToList();

                        //int id = temp2.ServiceId;
                        item[i].Street = address_obj[0].AddressLine1;
                        item[i].House_number = address_obj.FirstOrDefault().AddressLine2;
                        item[i].Zipcode = address_obj.FirstOrDefault().PostalCode;
                        item[i].Location = address_obj.FirstOrDefault().City;
                        item[i].Phone = address_obj.FirstOrDefault().Mobile;
                        i++;
                    }
                }
            }

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.data = item;
            var unme = HttpContext.Session.GetString("user");
            ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;

            return View();
        }

        public IActionResult Fetch_ES(int sid, int spid)
        {
            _helperlandcontext = new HelperlandContext();
            var obj = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == sid).ToList().FirstOrDefault();
            obj.ServiceProviderId = spid;
            obj.Status = 3;

            _helperlandcontext.Entry(obj).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            var zipcode = _helperlandcontext.Users.Where(x => x.UserId == spid).ToList().FirstOrDefault().ZipCode;
            var zipcode_sps = _helperlandcontext.Users.Where(x => x.ZipCode == zipcode).ToList();
            var email_sp = _helperlandcontext.Users.Where(x => x.UserId == spid).ToList().FirstOrDefault().Email;
            foreach (var temp in zipcode_sps)
            {
                var emailId = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList().FirstOrDefault().Email;
                if(emailId != email_sp)
                {
                    MailMessage mm = new MailMessage("pmmakadiya1@gmail.com", emailId);
                    mm.Subject = "New Service Request Accepted";
                    mm.Body = "Service Request with ID " + sid + " is no more Available, It is Accepted By One of the service Provider";
                        mm.IsBodyHtml = false;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    NetworkCredential nc = new NetworkCredential("pmmakadiya1@gmail.com", "123456789@gmail.com");
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = nc;
                    smtp.Send(mm);
                }
            }

            try
            {
                return Json(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult Upcoming_Services()
        {
            _helperlandcontext = new HelperlandContext();
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.ServiceProviderId == userid && x.Status == 3).ToList();
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.ServiceRequest temp in model_to_pass)
            {
                if (temp.ServiceRequestId >= 4)
                {
                    var duration = (temp.ServiceHours).ToString();
                    if (duration.Length <= 2)
                    {
                        duration += ":00";
                    }
                    else duration = Math.Round(temp.ServiceHours, 2).ToString() + '0';
                    var end_dur = Math.Round(double.Parse((temp.ServiceHours + temp.ExtraHours).ToString()), 2).ToString();
                    if (end_dur.ToString().Length <= 2)
                    {
                        duration = duration + "-" + end_dur.ToString() + ":00";
                    }
                    else duration = (duration + "-" + end_dur.ToString() + '0').Replace('.', ':');

                    var spid = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList();
                    int? spid3 = 0;
                    var name = "";

                    //var rating = 0;
                    if (spid.Count > 0)
                    {
                        spid3 = (spid.FirstOrDefault().UserId);
                        var fname = spid.FirstOrDefault().FirstName;
                        var lname = spid.FirstOrDefault().LastName;
                        name = fname + " " + lname;


                    }
                    int is_date_smaller = 0;
                    if((DateTime.Now.Year > temp.ServiceStartDate.Date.Year) || (DateTime.Now.Month > temp.ServiceStartDate.Month)
                        || (DateTime.Now.Day > temp.ServiceStartDate.Day))
                    {
                        is_date_smaller = 1;
                    }

                    item.Add(new Models.Book_now_Table
                    {
                        Is_Date_Smaller = is_date_smaller,
                        SP_ID = spid3,
                        SP_Name = name,
                        ID = temp.ServiceRequestId,
                        Booking_date = (temp.ServiceStartDate).Date,
                        Booking_time = (temp.ExtraHours).ToString(),
                        Discounted_cost = float.Parse((temp.SubTotal).ToString()),
                        Booking_duration = duration,
                        Suggestion = temp.Comments,
                        Status = temp.Status
                    });
                }
            }
            int i = 0;
            foreach (Models.ServiceRequest temp2 in model_to_pass)
            {
                if (temp2.ServiceRequestId >= 4)
                {


                    var address_obj = _helperlandcontext.ServiceRequestAddresses.Where(x => x.ServiceRequestId == temp2.ServiceRequestId).ToList();

                    //int id = temp2.ServiceId;
                    item[i].Street = address_obj[0].AddressLine1;
                    item[i].House_number = address_obj.FirstOrDefault().AddressLine2;
                    item[i].Zipcode = address_obj.FirstOrDefault().PostalCode;
                    item[i].Location = address_obj.FirstOrDefault().City;
                    item[i].Phone = address_obj.FirstOrDefault().Mobile;
                    i++;
                }
            }

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.data = item;
            var unme = HttpContext.Session.GetString("user");
            ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;


            return View();
        }

        public IActionResult Complete_srr(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();
            var obj = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            obj.Status = 1;

            _helperlandcontext.Entry(obj).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.is_service_accepted = "Thanks For completion of Service, Stay connected for Upcoming Services.";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Service_History()
        {
            _helperlandcontext = new HelperlandContext();
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.ServiceProviderId == userid && x.Status == 1).ToList();
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.ServiceRequest temp in model_to_pass)
            {
                if (temp.ServiceRequestId >= 4)
                {
                    var duration = (temp.ServiceHours).ToString();
                    if (duration.Length <= 2)
                    {
                        duration += ":00";
                    }
                    else duration = Math.Round(temp.ServiceHours, 2).ToString() + '0';
                    var end_dur = Math.Round(double.Parse((temp.ServiceHours + temp.ExtraHours).ToString()), 2).ToString();
                    if (end_dur.ToString().Length <= 2)
                    {
                        duration = duration + "-" + end_dur.ToString() + ":00";
                    }
                    else duration = (duration + "-" + end_dur.ToString() + '0').Replace('.', ':');

                    var spid = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList();
                    int? spid3 = 0;
                    var name = "";

                    //var rating = 0;
                    if (spid.Count > 0)
                    {
                        spid3 = (spid.FirstOrDefault().UserId);
                        var fname = spid.FirstOrDefault().FirstName;
                        var lname = spid.FirstOrDefault().LastName;
                        name = fname + " " + lname;


                    }
                    int is_date_smaller = 0;
                    if ((DateTime.Now.Year > temp.ServiceStartDate.Date.Year) || (DateTime.Now.Month > temp.ServiceStartDate.Month)
                        || (DateTime.Now.Day > temp.ServiceStartDate.Day))
                    {
                        is_date_smaller = 1;
                    }

                    item.Add(new Models.Book_now_Table
                    {
                        Is_Date_Smaller = is_date_smaller,
                        SP_ID = spid3,
                        SP_Name = name,
                        ID = temp.ServiceRequestId,
                        Booking_date = (temp.ServiceStartDate).Date,
                        Booking_time = (temp.ExtraHours).ToString(),
                        Discounted_cost = float.Parse((temp.SubTotal).ToString()),
                        Booking_duration = duration,
                        Suggestion = temp.Comments,
                        Status = temp.Status
                    });
                }
            }
            int i = 0;
            foreach (Models.ServiceRequest temp2 in model_to_pass)
            {
                if (temp2.ServiceRequestId >= 4)
                {


                    var address_obj = _helperlandcontext.ServiceRequestAddresses.Where(x => x.ServiceRequestId == temp2.ServiceRequestId).ToList();

                    //int id = temp2.ServiceId;
                    item[i].Street = address_obj[0].AddressLine1;
                    item[i].House_number = address_obj.FirstOrDefault().AddressLine2;
                    item[i].Zipcode = address_obj.FirstOrDefault().PostalCode;
                    item[i].Location = address_obj.FirstOrDefault().City;
                    item[i].Phone = address_obj.FirstOrDefault().Mobile;
                    i++;
                }
            }

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.data = item;
            var unme = HttpContext.Session.GetString("user");
            ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;


            return View();
        }

        public IActionResult Ratings()
        {
            _helperlandcontext = new HelperlandContext();
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var model_to_pass = _helperlandcontext.Ratings.Where(x => x.RatingTo == userid).ToList();
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.Rating temp2 in model_to_pass)
            {
                var temp = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == temp2.ServiceRequestId).ToList().FirstOrDefault();

                var duration = (temp.ServiceHours).ToString();
                if (duration.Length <= 2)
                {
                    duration += ":00";
                }
                else duration = Math.Round(temp.ServiceHours, 2).ToString() + '0';
                var end_dur = Math.Round(double.Parse((temp.ServiceHours + temp.ExtraHours).ToString()), 2).ToString();
                if (end_dur.ToString().Length <= 2)
                {
                    duration = duration + "-" + end_dur.ToString() + ":00";
                }
                else duration = (duration + "-" + end_dur.ToString() + '0').Replace('.', ':');

                var spid = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList();
                int? spid3 = 0;
                var name = "";

                //var rating = 0;
                if (spid.Count > 0)
                {
                    spid3 = (spid.FirstOrDefault().UserId);
                    var fname = spid.FirstOrDefault().FirstName;
                    var lname = spid.FirstOrDefault().LastName;
                    name = fname + " " + lname;


                }

                item.Add(new Models.Book_now_Table
                {
                    SP_Ratings = (float)temp2.Ratings,
                    SP_ID = spid3,
                    SP_Name = name,
                    ID = temp.ServiceRequestId,
                    Booking_date = (temp.ServiceStartDate).Date,
                    Booking_time = (temp.ExtraHours).ToString(),
                    Discounted_cost = float.Parse((temp.SubTotal).ToString()),
                    Booking_duration = duration,
                    Suggestion = temp2.Comments,
                    Status = temp.Status
                });
            }

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.data = item;
            var unme = HttpContext.Session.GetString("user");
            ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;

            return View();
        }

        public IActionResult Block_Customer()
        {
            HashSet<int> All_customers = new HashSet<int>();
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var all_ser = _helperlandcontext.ServiceRequests.Where(x => x.ServiceProviderId == userid && x.Status == 1).ToList();
            
            foreach(var temp in all_ser)
            {
                All_customers.Add(temp.UserId);
            }
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach(var i in All_customers)
            {
                var uname = _helperlandcontext.Users.Where(x => x.UserId == i).ToList().FirstOrDefault();
                var name = uname.FirstName + " " + uname.LastName;
                var is_blocked2 = 0;
                var is_blocked = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == userid && x.UserId == i).ToList();
                if(is_blocked.Count == 1)
                {
                    is_blocked2 = 1;
                }

                item.Add(new Models.Book_now_Table
                {
                    SP_ID = i,
                    SP_Name = name,
                    Is_Blocked = is_blocked2
                });
            }

            ViewBag.all_cus = item;
            var unme = HttpContext.Session.GetString("user");
            ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;
            return View();
        }

        public IActionResult Check_Block_Customer(Models.FavoriteAndBlocked model)
        {
            _helperlandcontext = new HelperlandContext();
           
            var is_blocked = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == model.TargetUserId && x.UserId == model.UserId).ToList();
            if(is_blocked.Count == 0)
            {
                Models.FavoriteAndBlocked obj2 = new Models.FavoriteAndBlocked();
                obj2.UserId = model.UserId;
                obj2.TargetUserId = model.TargetUserId;
                obj2.IsBlocked = true;
                obj2.IsFavorite = false;

                _helperlandcontext.FavoriteAndBlockeds.Add(obj2);
                _helperlandcontext.SaveChanges();
            }
            else
            {
                var bid = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == model.TargetUserId && x.UserId == model.UserId).ToList().FirstOrDefault().Id;
                var p = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.Id == bid).ToList().FirstOrDefault();
                _helperlandcontext.Entry(p).State = EntityState.Deleted;
                _helperlandcontext.SaveChanges();
            }
            var obj = "Hello";
            try
            {
                return Json(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
