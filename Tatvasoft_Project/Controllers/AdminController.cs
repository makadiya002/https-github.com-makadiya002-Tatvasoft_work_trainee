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

        public IActionResult Filter_User(Models.User model)
        {
            _helperlandcontext = new HelperlandContext();

            var filtered_users_fname = _helperlandcontext.Users.Where(x => true).ToList();
            var filtered_users_mobile = _helperlandcontext.Users.Where(x => true).ToList();
            var filtered_users_zipcode = _helperlandcontext.Users.Where(x => true).ToList();
            var filtered_users_type = _helperlandcontext.Users.Where(x => true).ToList();
            var filtered_users_createddate = _helperlandcontext.Users.Where(x => true).ToList();

            if (model.FirstName != null)
            {
                filtered_users_fname = _helperlandcontext.Users.Where(x => x.FirstName.Contains(model.FirstName)).ToList();
            }

            if(model.Mobile != null)
            {
                filtered_users_mobile = _helperlandcontext.Users.Where(x => x.Mobile.Contains(model.Mobile)).ToList();
            }

            if(model.ZipCode != null)
            {
                filtered_users_zipcode = _helperlandcontext.Users.Where(x => x.ZipCode.Contains(model.ZipCode)).ToList();
            }

            if(model.UserTypeId != 0)
            {
                filtered_users_type = _helperlandcontext.Users.Where(x => x.UserTypeId == model.UserTypeId).ToList();
            }

            if(model.DateOfBirth != null)
            {
                filtered_users_createddate = _helperlandcontext.Users.Where(x => x.CreatedDate == model.DateOfBirth).ToList();
            }


            var common_lst1 = filtered_users_fname.Intersect(filtered_users_mobile).ToList();
            var common_lst2 = filtered_users_zipcode.Intersect(filtered_users_type).ToList();

            var final_lst1 = common_lst1.Intersect(common_lst2).ToList();
            var final_lst = final_lst1.Intersect(filtered_users_createddate).ToList();

            ViewBag.All_filtered_rows = final_lst;
            return View();
            
        }

        public IActionResult Filter_Services(Models.Book_now_Table model, string Email, DateTime To_date)
        {
            ViewBag.is_default = "No its not!!";
            if (To_date.Year == 0001)
            {
                ViewBag.is_default = "Yes It is!!";
            }

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
                        Status = temp.Status,
                        //Zipcode = temp.ZipCode
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
                    item[i].Zipcode = temp2.ZipCode;
                    item[i].Location = address_obj.FirstOrDefault().City;
                    item[i].Phone = address_obj.FirstOrDefault().Mobile;
                    i++;

                }
            }

           
           
           

          
            List<Models.Book_now_Table> filtered_users_fname_cus = new List<Models.Book_now_Table>();
            List<Models.Book_now_Table> filtered_users_sp = new List<Models.Book_now_Table>();
            List<Models.Book_now_Table> filtered_users_sid = new List<Models.Book_now_Table>();
            List<Models.Book_now_Table> filtered_users_zipcode = new List<Models.Book_now_Table>();
            List<Models.Book_now_Table> filtered_users_email = new List<Models.Book_now_Table>();
            List<Models.Book_now_Table> filtered_users_status = new List<Models.Book_now_Table>();
            List<Models.Book_now_Table> filtered_users_date = new List<Models.Book_now_Table>();


            if (model.Customer_Name != null)
            {
                foreach (var tmp22 in item)
                {
                    if (tmp22.Customer_Name.Contains(model.Customer_Name))
                    {
                        filtered_users_fname_cus.Add(tmp22);
                    }
                }
            }
            else filtered_users_fname_cus = item;

            if (model.SP_Name != null)
            {
                foreach (var tmp22 in item)
                {
                    if (tmp22.SP_Name.Contains(model.SP_Name))
                    {
                        filtered_users_sp.Add(tmp22);
                    }
                }
            }
            else filtered_users_sp = item;

            if (model.ID != 0)
            {
                foreach (var tmp22 in item)
                {
                    if (tmp22.ID.ToString().Contains(model.ID.ToString()))
                    {
                        filtered_users_sid.Add(tmp22);
                    }
                }
            }
            else filtered_users_sid = item;

            if (model.Zipcode != null)
            {
                foreach (var tmp22 in item)
                {
                    if (tmp22.Zipcode.Contains(model.Zipcode))
                    {
                        filtered_users_zipcode.Add(tmp22);
                    }
                }
            }
            else filtered_users_zipcode = item;

            if (model.Status != 0)
            {
                foreach (var tmp22 in item)
                {
                    if(model.Status == 10)
                    {
                        if(tmp22.Status == null)
                        {
                            filtered_users_status.Add(tmp22);
                        }
                    }

                    else if (tmp22.Status == model.Status)
                    {
                        filtered_users_status.Add(tmp22);
                    }
                }
            }
            else filtered_users_status = item;

            if (To_date.Year != 0001 && model.Booking_date.Year != 0001)
            {
                foreach (var tmp22 in item)
                {
                    if (tmp22.Booking_date.CompareTo(model.Booking_date) >= 0 && tmp22.Booking_date.CompareTo(To_date) <= 0)
                    {
                        filtered_users_date.Add(tmp22);
                    }
                }
            }
            else filtered_users_date = item;

            if (Email != null)
            {
                foreach (var tmp22 in item)
                {
                    var mailid = _helperlandcontext.Users.Where(x => x.UserId == tmp22.SP_ID).ToList().FirstOrDefault().Email;
                    if (mailid.Contains(Email))
                    {
                        filtered_users_email.Add(tmp22);
                    }
                }
            }
            else filtered_users_email = item;

            //vus.sp,sid,zipcode

            var merged1 = filtered_users_fname_cus.Intersect(filtered_users_sp).ToList();
            var merged2 = filtered_users_sid.Intersect(filtered_users_zipcode).ToList();
            var merged3 = filtered_users_status.Intersect(filtered_users_date).ToList();
            var merged4 = merged1.Intersect(merged2).ToList();
            var merged5 = merged4.Intersect(merged3).ToList();

            ViewBag.data = merged5.Intersect(filtered_users_email).ToList();
            return View();

        }

        public IActionResult Edit_Service_Time_Address(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();

            //conflict code from here
            _helperlandcontext = new HelperlandContext();
            var useer = HttpContext.Session.GetString("user");
            var uidd = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault().ServiceProviderId;

            var all_services = _helperlandcontext.ServiceRequests.Where(x => x.Status == 3 && x.ServiceProviderId == uidd).ToList();
            var services = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            var service_date = model.Booking_date;
            var service_start_time = double.Parse(model.Booking_time);
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
                var service = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
                service.ServiceStartDate = model.Booking_date;
                service.ServiceHours = double.Parse(model.Booking_time);

                _helperlandcontext.Entry(service).State = EntityState.Modified;
                _helperlandcontext.SaveChanges();

                var service_address = _helperlandcontext.ServiceRequestAddresses.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();

                service_address.AddressLine1 = model.Street;
                service_address.AddressLine2 = model.House_number;
                service_address.City = model.Location;
                service_address.PostalCode = model.Zipcode;

                _helperlandcontext.Entry(service_address).State = EntityState.Modified;
                _helperlandcontext.SaveChanges();
                ViewBag.is_updated_address = "Service Details Edited Succesfully!";



                //send mail to user and sp
                var userr = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
                var userridd = userr.UserId;
                var SPID = userr.ServiceProviderId;
                var email = _helperlandcontext.Users.Where(x => x.UserId == userridd).ToList().FirstOrDefault().Email;

                //send mail to user
                MailMessage mm = new MailMessage("pmmakadiya1@gmail.com", email);
                mm.Subject = "Changes made by Admin";

                mm.Body = "Admin has updated details of service with id " + model.ID + " And, new date is " + model.Booking_date;
                mm.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential("pmmakadiya1@gmail.com", "123456789@gmail.com");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = nc;
                smtp.Send(mm);

                //send mail to service provider iff assigned
                if(userr.ServiceProviderId != null)
                {
                    var userr2 = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
                    //var userridd2 = userr2.UserId;
                    var SPID2 = userr2.ServiceProviderId;
                    var email2 = _helperlandcontext.Users.Where(x => x.UserId == SPID2).ToList().FirstOrDefault().Email;

                    MailMessage mm2 = new MailMessage("pmmakadiya1@gmail.com", email2);
                    mm2.Subject = "Changes made by Admin";

                    mm2.Body = "Admin has updated details of service with id " + model.ID + " And, new date is " + model.Booking_date;
                    mm2.IsBodyHtml = false;
                    SmtpClient smtp2 = new SmtpClient();
                    smtp2.Host = "smtp.gmail.com";
                    smtp2.Port = 587;
                    smtp2.EnableSsl = true;
                    NetworkCredential nc2 = new NetworkCredential("pmmakadiya1@gmail.com", "123456789@gmail.com");
                    smtp2.UseDefaultCredentials = true;
                    smtp2.Credentials = nc2;
                    smtp2.Send(mm2);

                }

            }
            else
            {
                ViewBag.is_service_conflict = "Can not Edit this service with selected date and time, Because it conflicts with another service of assigned service Provider";
            }


            //_helperlandcontext = new HelperlandContext();
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

            //sending mail code
            
            

            ViewBag.data = item;
            return View("Views/Admin/Service_Page.cshtml");
        }

        public IActionResult Cancel_sr2(Models.Book_now_Table model)
        {
            _helperlandcontext = new HelperlandContext();
            var q = _helperlandcontext.ServiceRequests.Where(x => x.ServiceRequestId == model.ID).ToList().FirstOrDefault();
            q.Status = 2;
            _helperlandcontext.Entry(q).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();

            ViewBag.is_service_Cancelled = "Service Cancelled succesfully!";
            ViewBag.user = HttpContext.Session.GetString("user");
            return RedirectToAction("Service_Page");
        }

        public IActionResult Check_Old_Password(Models.Password_Temp model)
        {
            _helperlandcontext = new HelperlandContext();
            var user = HttpContext.Session.GetString("user");
            var p = _helperlandcontext.Users.Where(x => x.FirstName == user).ToList().FirstOrDefault();
            bool is_pass_valid = BCrypt.Net.BCrypt.Verify(model.Password, p.Password);

            if (is_pass_valid == true) model.Password = "true";
            else model.Password = "false";

            try
            {
                return Json(model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
