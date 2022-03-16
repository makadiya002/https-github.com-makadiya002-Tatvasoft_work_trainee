﻿using Microsoft.AspNetCore.Mvc;
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
    public class AdminController : Controller
    {
        private HelperlandContext _helperlandcontext;
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Service_Page()
        {
            _helperlandcontext = new HelperlandContext();
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => true).ToList();
            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.ServiceRequest temp in model_to_pass)
            {

                if (temp.ServiceRequestId >= 4)
                {
                    var spid = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList();
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

                    var name_sp = "";
                    if (_helperlandcontext.Users.Where(x => x.UserId == temp.ServiceProviderId).ToList().Count > 0)
                    {
                        var spid_sp = _helperlandcontext.Users.Where(x => x.UserId == temp.ServiceProviderId).ToList().FirstOrDefault();
                        var sp_fn = _helperlandcontext.Users.Where(x => x.UserId == spid_sp.UserId).ToList().FirstOrDefault().FirstName;
                        var sp_ln = _helperlandcontext.Users.Where(x => x.UserId == spid_sp.UserId).ToList().FirstOrDefault().LastName;

                        name_sp = sp_fn + " " + sp_ln;
                    }
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
                        Customer_Name = name,
                        ID = temp.ServiceRequestId,
                        Booking_date = (temp.ServiceStartDate).Date,
                        Booking_time = (temp.ExtraHours).ToString(),
                        Discounted_cost = float.Parse((temp.SubTotal).ToString()),
                        Booking_duration = duration,
                        Suggestion = temp.Comments,
                        SP_Name = name_sp,
                        Status = temp.Status
                    });

                }
            }
            int i = 0;
            foreach (Models.ServiceRequest temp2 in model_to_pass)
            {
                if (temp2.ServiceRequestId >= 4)
                {
                    var spid = _helperlandcontext.Users.Where(x => x.UserId == temp2.UserId).ToList();

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

            //ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.data = item;
            //var unme = HttpContext.Session.GetString("user");
            //ViewBag.uid = _helperlandcontext.Users.Where(x => x.FirstName == unme).ToList().FirstOrDefault().UserId;

            return View();
        }
        public IActionResult Customer_Page()
        {
            _helperlandcontext = new HelperlandContext();
            var all_users = _helperlandcontext.Users.Where(x => true).ToList();
            List<Models.User> item = new List<Models.User>();
            foreach(var temp in all_users)
            {
                var name = "";
                var fn = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList().FirstOrDefault().FirstName;
                var ln = _helperlandcontext.Users.Where(x => x.UserId == temp.UserId).ToList().FirstOrDefault().LastName;
                name = fn + " " + ln;
                item.Add(new Models.User
                {
                    FirstName = name,
                    UserTypeId = temp.UserTypeId,
                    CreatedDate = (temp.CreatedDate).Date,
                    Mobile = temp.Mobile,
                    ZipCode = temp.ZipCode,
                    IsActive = temp.IsActive,
                    UserId = temp.UserId,
                    IsApproved = temp.IsApproved
                });
            }
            ViewBag.data = item;
            return View();
        }

        public IActionResult Deactivate_user(int UserId)
        {
            _helperlandcontext = new HelperlandContext();
            var user = _helperlandcontext.Users.Where(x => x.UserId == UserId).ToList().FirstOrDefault();
            user.IsActive = false;

            _helperlandcontext.Entry(user).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            try
            {
                return Json(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public IActionResult Activate_user(int UserId)
        {
            _helperlandcontext = new HelperlandContext();
            var user = _helperlandcontext.Users.Where(x => x.UserId == UserId).ToList().FirstOrDefault();
            user.IsActive = true;

            _helperlandcontext.Entry(user).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            try
            {
                return Json(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult Approve_SP(int UserId)
        {
            _helperlandcontext = new HelperlandContext();
            var obj = _helperlandcontext.Users.Where(x => x.UserId == UserId).ToList().FirstOrDefault();
            obj.IsApproved = true;

            _helperlandcontext.Entry(obj).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

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
