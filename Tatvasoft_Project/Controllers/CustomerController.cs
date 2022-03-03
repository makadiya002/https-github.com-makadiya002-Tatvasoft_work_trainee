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
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Delete_Address_from_modal(Models.UserAddress model)
        {
            _helperlandcontext = new HelperlandContext();
            var p = (_helperlandcontext.UserAddresses.Where(x => x.AddressId == model.AddressId).ToList()).FirstOrDefault();
            _helperlandcontext.Entry(p).State = EntityState.Deleted;
            _helperlandcontext.SaveChanges();

            ViewBag.is_address_deleted_fm = "Address deleted succesfully!";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Password_Edit(Models.Password_Temp model)
        {
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var p = (_helperlandcontext.Users.Where(x => x.UserId == userid).ToList()).FirstOrDefault();
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
                return View("~/Views/Customer/Profile_Change_Pass2.cshtml");
            }
            ViewBag.pass_changed_fp = "Password Changed Succesfully, You can login again!";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Dashboard()
        {
            var username = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && x.Status == null).ToList();
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
                    
                    item.Add(new Models.Book_now_Table
                    {

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
            var p = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            p.ServiceStartDate = model.Booking_date;
            p.ServiceHours = double.Parse(model.Booking_time);

            _helperlandcontext.Entry(p).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            ViewBag.is_service_modified = "Details Updated succesfully!";
            ViewBag.user = HttpContext.Session.GetString("user");
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Cancel_sr(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();
            var p = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            p.Status = 2;
            _helperlandcontext.Entry(p).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            ViewBag.is_service_Cancelled = "Service Cancelled succesfully!";
            ViewBag.user = HttpContext.Session.GetString("user");
            return View("~/Views/Home/Index.cshtml");
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
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && x.Status != null).ToList();
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
            return View();
        }



        public ViewResult demo2()
        {
            return View();
        }

    }
    }

