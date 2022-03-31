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
    public class CustomerController : Controller
    {
        private HelperlandContext _helperlandcontext;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile_details()
        {
            var username =  HttpContext.Session.GetString("user");
            ViewBag.user = username;
            _helperlandcontext = new HelperlandContext();
            var p = _helperlandcontext.Users.Where(x => x.FirstName == username).ToList();
            //Models.User data = new Models.User();
            ViewBag.date = (p.FirstOrDefault().DateOfBirth).ToString();
            var actual_date = "";
            var a = 0;
            foreach(var x in ViewBag.date)
            {
                a++;
                if (a <= 10)
                    actual_date = actual_date + x;
                else break;
            }
            ViewBag.actual_date = actual_date;
            ViewBag.data = p.FirstOrDefault();

            return View();
        }
        public IActionResult Profile_Address()
        {
            _helperlandcontext = new HelperlandContext();
            var username = HttpContext.Session.GetString("user");
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var usertypeid = _helperlandcontext.Users.Where(x => x.FirstName == username).ToList().FirstOrDefault().UserTypeId;
            var model_to_pass = _helperlandcontext.UserAddresses.Where(x => x.UserId == userid).ToList();

            List<Models.UserAddress> item = new List<Models.UserAddress>();
            foreach (Models.UserAddress temp in model_to_pass)
            {
                item.Add(new Models.UserAddress
                {
                    AddressLine1 = temp.AddressLine1,
                    AddressLine2 = temp.AddressLine2,
                    City = temp.City,
                    PostalCode = temp.PostalCode,
                    Mobile = temp.Mobile,
                    AddressId = temp.AddressId
                });
            }

            if(usertypeid == 2)
            {
                ViewBag.from_SP_side = "Yes";
            }
            ViewBag.all_address = item;
            return View();
        }
        public IActionResult Profile_Change_Pass()
        { 
            return View();
        }

        public IActionResult Details_Edit(Models.User m)
        {
            _helperlandcontext = new HelperlandContext();
            Models.User model = (_helperlandcontext.Users.Where(x => x.UserId == m.UserId).ToList()).FirstOrDefault();
            model.UserId = m.UserId;
            model.FirstName = m.FirstName;
            model.LastName = m.LastName;
            model.Email = m.Email;
            model.Mobile = m.Mobile;
            model.DateOfBirth = m.DateOfBirth;
           
            _helperlandcontext.Entry(model).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();


            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.is_details_update = "Details Updated Succesfully!";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Edit_Address_from_modal(Models.UserAddress model)
        {
            _helperlandcontext = new HelperlandContext();
            var p = (_helperlandcontext.UserAddresses.Where(x => x.AddressId == model.AddressId).ToList()).FirstOrDefault();
            p.AddressLine1 = model.AddressLine1;
            p.AddressLine2 = model.AddressLine2;
            p.City = model.City;
            p.PostalCode = model.PostalCode;
            p.Mobile = model.Mobile;
            _helperlandcontext.Entry(p).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            ViewBag.is_address_updated_fm = "Address updated succesfully!";
            ViewBag.user = HttpContext.Session.GetString("user");
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Delete_Address_from_modal(Models.UserAddress model)
        {
            _helperlandcontext = new HelperlandContext();
            var p = (_helperlandcontext.UserAddresses.Where(x => x.AddressId == model.AddressId).ToList()).FirstOrDefault();
            _helperlandcontext.Entry(p).State = EntityState.Deleted;
            _helperlandcontext.SaveChanges();

            ViewBag.is_address_deleted_fm = "Address deleted succesfully!";
            ViewBag.user = HttpContext.Session.GetString("user");
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Password_Edit(Models.Password_Temp model)
        {
            var username2 = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid2 = (_helperlandcontext.Users.Where(x => x.FirstName == username2).ToList()).FirstOrDefault().UserId;
            var p = (_helperlandcontext.Users.Where(x => x.UserId == userid2).ToList()).FirstOrDefault();
            bool is_pass_valid = BCrypt.Net.BCrypt.Verify(model.Password, p.Password);
            //Models.User m1 = new Models.User();
            //m1.Email = p.FirstOrDefault().Email;

            if (is_pass_valid == true)
            {
                p.Password = BCrypt.Net.BCrypt.HashPassword(model.Confirm_Password);
                _helperlandcontext.Entry(p).State = EntityState.Modified;
                _helperlandcontext.SaveChanges();

            }
            else
            {
                ViewBag.is_pass_wrong = "Old Password is incorrect!";
                var username = HttpContext.Session.GetString("user");
                _helperlandcontext = new HelperlandContext();
                var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
                var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && (x.Status == null) || x.Status == 3).ToList();
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

                        var spid = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == temp.ServiceRequestId && temp.ServiceProviderId != null).ToList();
                        int? spid3 = 0;
                        var name = "";

                        var rating = 0;
                        if (spid.Count > 0)
                        {
                            spid3 = (spid.FirstOrDefault().ServiceProviderId);
                            var fname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().FirstName;
                            var lname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().LastName;
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
                return View("~/Views/Customer/Dashboard.cshtml");
            }
            ViewBag.pass_changed_fp = "Password Changed Succesfully";
            ViewBag.user = HttpContext.Session.GetString("user");
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;


            var usertypeid = _helperlandcontext.Users.Where(x => x.UserId == userid).ToList().FirstOrDefault().UserTypeId;
            if(usertypeid == 2)
            {
                return Redirect("/Service_Provider/SP_Dashboard");
            }



            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && ((x.Status == null) || x.Status == 3)).ToList();
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.ServiceRequest temp in model_to_pass)
            {
                if(temp.ServiceRequestId >= 4)
                {
                    var duration = (temp.ServiceHours).ToString();
                    if (duration.Length <= 2)
                    {
                        duration += ":00";
                    }
                    else duration = Math.Round(temp.ServiceHours, 2).ToString() + '0';
                    var end_dur = Math.Round(double.Parse((temp.ServiceHours + temp.ExtraHours).ToString()), 2).ToString();
                    if(end_dur.ToString().Length <= 2)
                    {
                        duration = duration + "-" + end_dur.ToString() + ":00";
                    }
                    else duration = (duration + "-" + end_dur.ToString() + '0').Replace('.',':');

                    var spid = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == temp.ServiceRequestId && temp.ServiceProviderId != null).ToList();
                    int? spid3 = 0;
                    var name = "";

                    var rating = 0;
                    if (spid.Count > 0)
                    {
                        spid3 = (spid.FirstOrDefault().ServiceProviderId);
                        var fname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().FirstName;
                        var lname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().LastName;
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

                    });
                }
            }
            int i = 0;
            foreach(Models.ServiceRequest temp2 in model_to_pass)
            {
               if(temp2.ServiceRequestId >= 4)
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
            return View();
        }

        public IActionResult Modify_Service_dt(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();
            var useer = HttpContext.Session.GetString("user");
            var uidd = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault().ServiceProviderId;

            var all_services = _helperlandcontext.ServiceRequests.Where(x => x.Status == 3 && x.ServiceProviderId == uidd).ToList();
            var services = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            var service_date = model.Booking_date;
            var service_start_time = double.Parse(model.Booking_time) ;
            var service_end_time = services.ExtraHours + double.Parse(model.Booking_time);

            Models.ServiceRequest obj = new Models.ServiceRequest();
            obj.PaymentDue = true;

            foreach (var tmpp in all_services)
            {
                if (tmpp.ServiceStartDate == service_date && tmpp.ServiceRequestId != model.ID)
                {
                    var start_time = tmpp.ServiceHours;
                    var end_time = tmpp.ExtraHours + tmpp.ServiceHours;
                    if (service_start_time >= (start_time - 1) && service_start_time <= (end_time + 1))
                    {
                        obj.PaymentDue = false;
                    }
                    else if (service_start_time >= (start_time - 1) && service_end_time <= (end_time + 1))
                    {
                        obj.PaymentDue = false;
                    }
                    else if (service_start_time <= (start_time - 1) && service_end_time >= (start_time - 1))
                    {
                        obj.PaymentDue = false;
                    }
                    else if (service_start_time <= (start_time - 1) && service_end_time >= (end_time + 1))
                    {
                        obj.PaymentDue = false;
                    }



                }
            }



            if (obj.PaymentDue == true)
            {
                var p = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
                p.ServiceStartDate = model.Booking_date;
                p.ServiceHours = double.Parse(model.Booking_time);

                _helperlandcontext.Entry(p).State = EntityState.Modified;
                _helperlandcontext.SaveChanges();

                ViewBag.is_service_modified = "Details Updated succesfully!";

                var servic3 = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
                if(servic3.ServiceProviderId != null)
                {
                    var userr = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
                    var userridd = userr.UserId;
                    var SPID = userr.ServiceProviderId;
                    var email = _helperlandcontext.Users.Where(x => x.UserId == SPID).ToList().FirstOrDefault().Email;

                    //send mail to user
                    MailMessage mm = new MailMessage("pmmakadiya1@gmail.com", email);
                    mm.Subject = "Changes made by customer in Service";

                    mm.Body = "Customer has updated details of service with ID " + model.ID + " And, new date is " + model.Booking_date;
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
            else
            {
                //ViewBag.user = HttpContext.Session.GetString("user");
                ViewBag.is_conflict = "This service can't be updated to selected date and time, Because it conflicts with date and time of another service of your service Provider";
            }

            //new added from here
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;


            var usertypeid = _helperlandcontext.Users.Where(x => x.UserId == userid).ToList().FirstOrDefault().UserTypeId;
            if (usertypeid == 2)
            {
                return Redirect("/Service_Provider/SP_Dashboard");
            }



            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && ((x.Status == null) || x.Status == 3)).ToList();
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

                    var spid = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == temp.ServiceRequestId && temp.ServiceProviderId != null).ToList();
                    int? spid3 = 0;
                    var name = "";

                    var rating = 0;
                    if (spid.Count > 0)
                    {
                        spid3 = (spid.FirstOrDefault().ServiceProviderId);
                        var fname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().FirstName;
                        var lname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().LastName;
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


            return View("~/Views/Customer/Dashboard.cshtml");
        }

        public IActionResult Cancel_sr(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();
            var q = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            q.Status = 2;
            _helperlandcontext.Entry(q).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            ViewBag.is_service_Cancelled = "Service Cancelled succesfully!";
            ViewBag.user = HttpContext.Session.GetString("user");

            var fname_user = HttpContext.Session.GetString("user");
            var usertypeid = _helperlandcontext.Users.Where(x => x.FirstName == fname_user).ToList().FirstOrDefault().UserTypeId;

            if(usertypeid == 2)
            {
                return Redirect("/Service_Provider/SP_Dashboard");
            }

            return RedirectToAction("Dashboard");
        }

        
        public IActionResult Fetch_Address(Models.ServiceRequestExtra model)
        {
            _helperlandcontext = new HelperlandContext();
            var all_es = _helperlandcontext.ServiceRequestExtras.Where(x => x.ServiceRequestId == model.ServiceRequestId).ToList();
            Models.Book_now_Table data = new Models.Book_now_Table();
            data.Laundry = 0;
            data.Oven = 0;
            data.Windows = 0;
            data.Cabinate = 0;
            data.Fridge = 0;

            if(all_es.Count > 0)
            {
                foreach (var temp in all_es)
                {
                    if (temp.ServiceExtraId == 1)
                    { data.Laundry = 1; }
                    if (temp.ServiceExtraId == 2)
                    { data.Fridge = 1; }
                    if (temp.ServiceExtraId == 3)
                    { data.Cabinate = 1; }
                    if (temp.ServiceExtraId == 4)
                    { data.Oven = 1; }
                    if (temp.ServiceExtraId == 5)
                    { data.Windows = 1; }
                }
            }

            try
            {
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult Get_Done_Services()
        {
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && (x.Status == 1 || x.Status == 2)).ToList();
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

                    var spid = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == temp.ServiceRequestId && temp.ServiceProviderId != null).ToList();
                    int? spid3=0;
                    var name = "";
                   
                    var rating = 0;
                    if (spid.Count > 0)
                    {
                        spid3 = (spid.FirstOrDefault().ServiceProviderId);
                        var fname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().FirstName;
                        var lname = _helperlandcontext.Users.Where(x => x.UserId == spid3).ToList().FirstOrDefault().LastName;
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
                    }) ;
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

        public IActionResult Provide_Ratings(Models.Rating model)
        {
            Models.Rating data = new Models.Rating();
            _helperlandcontext = new HelperlandContext();

            data.ServiceRequestId = model.ServiceRequestId;
            data.RatingFrom = model.RatingFrom;
            data.RatingTo = model.RatingTo;
            data.Ratings = model.Ratings;
            data.Comments = model.Comments;
            data.RatingDate = DateTime.Now;
            data.VisibleOnHomeScreen = false;
            data.OnTimeArrival = model.OnTimeArrival;
            data.Friendly = model.Friendly;
            data.QualityOfService = model.QualityOfService;

            _helperlandcontext.Ratings.Add(data);
            _helperlandcontext.SaveChanges();


            try
            {
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult contact_Form(Models.ContactU model)
        {
            _helperlandcontext = new HelperlandContext();
            model.IsDeleted = false;
            _helperlandcontext.ContactUs.Add(model);
            _helperlandcontext.SaveChanges();

            ViewBag.is_entry_done = "Form Submitted succesfully, we will contact You ASAP";
            return View("~/Views/Helperland/Contact.cshtml");
        }

        public IActionResult Bn_Add_Address(Models.UserAddress model)
        {
            _helperlandcontext = new HelperlandContext();
            var uuname = HttpContext.Session.GetString("user");
            model.UserId = _helperlandcontext.Users.Where(x => x.FirstName == uuname).ToList().FirstOrDefault().UserId;

            _helperlandcontext.UserAddresses.Add(model);
            _helperlandcontext.SaveChanges();

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.is_add_added = "Address Added Succesfully";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Favourite_SP()
        {
            HashSet<int?> All_SP = new HashSet<int?>();
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var all_ser = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && x.Status == 1).ToList();

            foreach (var temp in all_ser)
            {
                if(temp.ServiceProviderId != null)
                {
                    All_SP.Add(temp.ServiceProviderId);
                }
            }
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (var i in All_SP)
            {
                var uname = _helperlandcontext.Users.Where(x => x.UserId == i).ToList().FirstOrDefault();
                var name = uname.FirstName + " " + uname.LastName;
                var is_blocked2 = 0;
                var is_fav = 0;
                var is_fav2 = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == userid && x.UserId == i && x.IsFavorite == true).ToList();
                if (is_fav2.Count == 1)
                {
                    is_fav = 1;
                }
                var is_blocked = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == userid && x.UserId == i && x.IsBlocked == true).ToList();
                if (is_blocked.Count == 1)
                {
                    is_blocked2 = 1;
                }

                item.Add(new Models.Book_now_Table
                {
                    SP_ID = i,
                    SP_Name = name,
                    Is_Blocked = is_blocked2,
                    Is_Fav = is_fav
                });
            }

            ViewBag.all_SP = item;
            var unme = HttpContext.Session.GetString("user");
            ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;
            ViewBag.user = HttpContext.Session.GetString("user");
            return View();
        }

        public IActionResult Block_Unblock_SP(Models.ServiceRequest model)
        {
            _helperlandcontext = new HelperlandContext();

            var uname = HttpContext.Session.GetString("user");
            var uid = _helperlandcontext.Users.Where(x => x.FirstName == uname).ToList().FirstOrDefault().UserId;
            var is_blocked = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == uid && x.UserId == model.ServiceProviderId && x.IsBlocked == true).ToList();
            if (is_blocked.Count == 0)
            {
                Models.FavoriteAndBlocked obj2 = new Models.FavoriteAndBlocked();
                obj2.UserId = int.Parse(model.ServiceProviderId.ToString());
                obj2.TargetUserId = uid;
                obj2.IsBlocked = true;
                obj2.IsFavorite = false;

                _helperlandcontext.FavoriteAndBlockeds.Add(obj2);
                _helperlandcontext.SaveChanges();
            }
            else
            {
                var bid = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == uid && x.UserId == model.ServiceProviderId && x.IsBlocked == true).ToList().FirstOrDefault().Id;
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

        public IActionResult Make_SP_Fav_Unfav(Models.ServiceRequest model)
        {
            _helperlandcontext = new HelperlandContext();

            var uname = HttpContext.Session.GetString("user");
            var uid = _helperlandcontext.Users.Where(x => x.FirstName == uname).ToList().FirstOrDefault().UserId;
            var is_blocked = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == uid && x.UserId == model.ServiceProviderId && x.IsFavorite == true).ToList();
            if (is_blocked.Count == 0)
            {
                Models.FavoriteAndBlocked obj2 = new Models.FavoriteAndBlocked();
                obj2.UserId = int.Parse(model.ServiceProviderId.ToString());
                obj2.TargetUserId = uid;
                obj2.IsBlocked = false;
                obj2.IsFavorite = true;

                _helperlandcontext.FavoriteAndBlockeds.Add(obj2);
                _helperlandcontext.SaveChanges();
            }
            else
            {
                var bid = _helperlandcontext.FavoriteAndBlockeds.Where(x => x.TargetUserId == uid && x.UserId == model.ServiceProviderId && x.IsFavorite == true).ToList().FirstOrDefault().Id;
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

        public ViewResult demo2()
        {
            return View();
        }

    }
    }

