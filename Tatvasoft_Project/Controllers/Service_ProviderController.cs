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
            var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.ZipCode == userid && x.Status == null).ToList();
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

        public IActionResult Fetch_ES(int sid, int spid)
        {
            _helperlandcontext = new HelperlandContext();
            var obj = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == sid).ToList().FirstOrDefault();
            obj.ServiceProviderId = spid;
            obj.Status = 3;

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
