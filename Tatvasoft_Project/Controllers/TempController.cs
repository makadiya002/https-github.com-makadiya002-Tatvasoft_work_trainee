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
    public class TempController : Controller
    {
        private HelperlandContext _helperlandcontext;

       
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GoogleMap()
        {
            return View();
        }

        public IActionResult Test_schedule()
        {

            
             _helperlandcontext = new HelperlandContext();
             var username = HttpContext.Session.GetString("user");
             _helperlandcontext = new HelperlandContext();
             var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;

             //var targetuserid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
             var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.ServiceProviderId == userid && x.Status == 3).ToList();
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
                             Date_String = (temp.ServiceStartDate).ToString("yyyy-MM-dd"),
                             Booking_date = (temp.ServiceStartDate).Date,
                             Booking_time = (temp.ExtraHours).ToString(),
                             Discounted_cost = float.Parse((temp.SubTotal).ToString()),
                             Booking_duration = duration,
                             Suggestion = temp.Comments,
                             Status = temp.Status
                         });

                    

                 }
             }

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.all_ss = item;
            //ViewBag.all_ss2 = item2;
            return View();
        }

        public IActionResult Fetch_Address_Details(Models.ServiceRequestAddress model)
        {
            _helperlandcontext = new HelperlandContext();
            var all_services = _helperlandcontext.ServiceRequestAddresses.Where(x => x.ServiceRequestId == model.ServiceRequestId).ToList().FirstOrDefault();
            //all_services
            try
            {
                return Json(all_services);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult check_Conflict_Book_Service(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();
            var useer = HttpContext.Session.GetString("user");
            //var uidd = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault().ServiceProviderId;
            var uidd = model.SP_ID;
            var all_services = _helperlandcontext.ServiceRequests.Where(x => x.Status == 3 && x.ServiceProviderId == uidd).ToList();
            //var services = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            var service_date = DateTime.Parse(HttpContext.Session.GetString("Booking_date"));
            var service_start_time = float.Parse(HttpContext.Session.GetString("Booking_time"));
            var service_end_time = double.Parse(HttpContext.Session.GetString("Booking_duration")) + service_start_time;

            Models.ServiceRequest obj = new Models.ServiceRequest();
            obj.PaymentDue = true;

            foreach (var tmpp in all_services)
            {
                if (tmpp.ServiceStartDate == service_date)
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
           //Models.ServiceRequest obj = new Models.ServiceRequest();
            try
            {
                return Json(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult Get_Service_Date(Models.ServiceRequest model)
        {
            _helperlandcontext = new HelperlandContext();
            var service = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ServiceRequestId).ToList().FirstOrDefault();
            var start_date = service.ServiceStartDate.Date.ToShortDateString();
            try
            {
                return Json(start_date);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
