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
    public class HelperlandController : Controller
    {
        private HelperlandContext _helperlandcontext;
        //private Models.Session ssn;
        //private object userManager;


        public object Session { get; set; }
        
        

        public IActionResult Index()
        {
            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.sugg = HttpContext.Session.GetString("Booking_instruction");
            return View("~/Views/Home/Index.cshtml");

        }

        public ViewResult Prices()
        {
            ViewBag.user = HttpContext.Session.GetString("user");

            return View();
        }

        public ViewResult Contact()
        {
            ViewBag.user = HttpContext.Session.GetString("user");
            return View();
        }

        public ViewResult FAQ()
        {
            ViewBag.user = HttpContext.Session.GetString("user");
            return View();
        }

        public ViewResult About()
        {
            ViewBag.user = HttpContext.Session.GetString("user");
            return View();
        }
        public ViewResult Become_helper()
        {
            ViewBag.user = HttpContext.Session.GetString("user");
            return View();
        }
        public ViewResult New_user()
        {
            return View();
        }
        public IActionResult Register(Models.User model)
        {
            Models.User obj = new Models.User();
            obj.FirstName = model.FirstName;
            obj.LastName = model.LastName;
            obj.Email = model.Email;
            obj.Mobile = model.Mobile;
            obj.ZipCode = model.ZipCode;
            obj.UserTypeId = 1;
            obj.IsRegisteredUser = true;
            obj.WorksWithPets = model.WorksWithPets;
            obj.DateOfBirth = model.DateOfBirth;
            obj.CreatedDate = DateTime.Today;
            obj.ModifiedDate = DateTime.Today;
            obj.ModifiedBy = 1;
            obj.IsApproved = true;
            obj.IsActive = true;
            obj.IsDeleted = true;
            obj.IsOnline = true;
            var key = Guid.NewGuid();
            obj.ResetKey = "h" + key;
            _helperlandcontext = new HelperlandContext();
            obj.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var p = _helperlandcontext.Users.Where(x => x.Email == model.Email).ToList();
            if (p.Count >= 1)
            {
                ViewBag.err = "Email ID Already Exist, Please use different Email ID";
                return View("~/Views/Helperland/New_user.cshtml");
            }
            var user = model.FirstName;
            HttpContext.Session.SetString("user", user);
            ViewBag.user = HttpContext.Session.GetString("user");
            _helperlandcontext.Users.Add(obj);
            _helperlandcontext.SaveChanges();
            return View();
        }

        public IActionResult Register_sp(Models.User model)
        {
            Models.User obj = new Models.User();
            obj.FirstName = model.FirstName;
            obj.LastName = model.LastName;
            obj.Email = model.Email;
            obj.Mobile = model.Mobile;
            obj.ZipCode = model.ZipCode;
            obj.UserTypeId = 2;
            obj.IsRegisteredUser = true;
            obj.WorksWithPets = false;
            obj.DateOfBirth = model.DateOfBirth;
            obj.CreatedDate = DateTime.Today;
            obj.ModifiedDate = DateTime.Today;
            obj.ModifiedBy = 2;
            obj.IsApproved = true;
            obj.IsActive = true;
            obj.IsDeleted = true;
            obj.IsOnline = true;
            _helperlandcontext = new HelperlandContext();
            obj.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            obj.ResetKey = (Guid.NewGuid()).ToString();
            var p = _helperlandcontext.Users.Where(x => x.Email == model.Email).ToList();
            if (p.Count >= 1)
            {
                ViewBag.err = "Email ID Already Exist, Please use different Email ID";
                return View("~/Views/Helperland/Become_helper.cshtml");
            }
            var user = model.FirstName;
            HttpContext.Session.SetString("user", user);
            ViewBag.user = HttpContext.Session.GetString("user");
            _helperlandcontext.Users.Add(obj);
            _helperlandcontext.SaveChanges();
            return View();
        }

        public IActionResult Login(Models.User model)
        {
            _helperlandcontext = new HelperlandContext();
            string email = model.Email;
            var p = _helperlandcontext.Users.Where(x => x.Email == email).ToList();
            
            if (p.Count == 1)
            {
                bool is_pass_valid = BCrypt.Net.BCrypt.Verify(model.Password, p.FirstOrDefault().Password);
                if(is_pass_valid)
                {
                    if (p.FirstOrDefault().UserTypeId == 1)
                    {
                        var user = p.FirstOrDefault().FirstName;
                        HttpContext.Session.SetString("user", user);
                        var username = HttpContext.Session.GetString("user");
                        _helperlandcontext = new HelperlandContext();
                        var userid = (_helperlandcontext.Users.Where(x => x.FirstName == username).ToList()).FirstOrDefault().UserId;
                        var model_to_pass = _helperlandcontext.ServiceRequests.Where(x => x.UserId == userid && x.Status != 2).ToList();
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
                    else if (p.FirstOrDefault().UserTypeId == 2)
                    {
                        var user = p.FirstOrDefault().FirstName;
                        HttpContext.Session.SetString("user", user);
                        ViewBag.user = HttpContext.Session.GetString("user");
                        return View("~/Views/Helperland/Register_sp.cshtml");
                    }
                }
                else ViewBag.err = "Something went wrong";
                return View("~/Views/Home/Index.cshtml");
            }
            else ViewBag.err = "Something went wrong";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Forgot_password(Models.User model)
        {
            var email = model.Email;
            MailMessage mm = new MailMessage("pmmakadiya1@gmail.com", email);
            mm.Subject = "Reset Your Password from Helperland";
            _helperlandcontext = new HelperlandContext();
            var p = _helperlandcontext.Users.Where(x => x.Email == email).ToList();
            if (p.Count >= 1)
            {
                var guid = p.FirstOrDefault().UserId;

                mm.Body = "https://localhost:44385/Helperland/Reset_password/" + guid + "  " + "You can set your new password by clicking on given link";
                mm.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential("pmmakadiya1@gmail.com", "123456789@gmail.com");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = nc;
                smtp.Send(mm);
                return View();
            }
            else
            {
                ViewBag.err_email = "Email not Registered!";
                return View("~/Views/Home/Index.cshtml");
            }
            
        }

      
        public IActionResult Reset_password(int id)
        {
            _helperlandcontext = new HelperlandContext();
            Models.User model = (_helperlandcontext.Users.Where(x => x.UserId == id).ToList()).FirstOrDefault();
            ViewBag.key = id;
            return View();
        }

        
        public IActionResult Reset_pass(Models.User m)
        {
            _helperlandcontext = new HelperlandContext();
            Models.User model = (_helperlandcontext.Users.Where(x => x.UserId == m.UserId).ToList()).FirstOrDefault();
            model.UserId = m.UserId;
            model.Password = BCrypt.Net.BCrypt.HashPassword(m.Password);

            _helperlandcontext.Entry(model).State = EntityState.Modified;
            _helperlandcontext.SaveChanges();
            ViewBag.msg = "Password Reset Succesful, Now you can login to your account.";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("user");
            ViewBag.logout = "Logout Succesfully!";
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Booknow()
        {
            ViewBag.Logged_user = HttpContext.Session.GetString("user");
            ViewBag.zipcode_found = "no";
            ViewBag.from_main = "yes";
            return View();
        }

        public IActionResult Booknow_first(Models.Book_now_Table model)
        {
            var zipcode = model.Zipcode;
            HttpContext.Session.SetString("zipcode", model.Zipcode);
            _helperlandcontext = new HelperlandContext();
            var p = _helperlandcontext.Users.Where(x => x.ZipCode == zipcode && x.UserTypeId == 2).ToList();
            if(p.Count > 0)
            {
                ViewBag.zipcode_found = "yes";
            }
            else
            {
                ViewBag.zipcode_found = "no";
            }
            ViewBag.first_tab_done = "yes";
            ViewBag.Logged_user = HttpContext.Session.GetString("user");
            return View("~/Views/Helperland/Booknow.cshtml");
        }
        
        public IActionResult Booknow_second(Models.Book_now_Table model)
        {

            HttpContext.Session.SetString("Booking_date", (model.Booking_date).ToString());
            HttpContext.Session.SetString("Booking_time", model.Booking_time);
            HttpContext.Session.SetString("Booking_duration", model.Booking_duration);
            HttpContext.Session.SetString("Laundry", (model.Laundry).ToString());
            HttpContext.Session.SetString("Fridge", (model.Fridge).ToString());
            HttpContext.Session.SetString("Cabinate", (model.Cabinate).ToString());
            HttpContext.Session.SetString("Oven", (model.Oven).ToString());
            HttpContext.Session.SetString("Windows", (model.Windows).ToString());

           
            if(model.Suggestion != null)
            {
                HttpContext.Session.SetString("Suggestion", model.Suggestion);
            }
            HttpContext.Session.SetString("Pets", (model.Pets).ToString());

            ViewBag.suggestions = HttpContext.Session.GetString("Booking_instruction");
            _helperlandcontext = new HelperlandContext();

            var str = HttpContext.Session.GetString("user");
            var p = _helperlandcontext.Users.Where(x => x.FirstName == str).ToList();
            var user = p.FirstOrDefault().UserId;
            var zcode = HttpContext.Session.GetString("zipcode");
            var model_to_pass = _helperlandcontext.UserAddresses.Where(x => x.UserId == user && x.PostalCode == zcode).ToList();

            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach (Models.UserAddress temp in model_to_pass)
            {
                item.Add(new Models.Book_now_Table
                {
                    Street = temp.AddressLine1,
                    House_number = temp.AddressLine2,
                    Location = temp.City,
                    Zipcode = temp.PostalCode,
                    Phone = temp.Mobile,
                    ID = temp.AddressId
                });
            }
            
            ViewBag.main_hrs = float.Parse(model.Booking_duration);
            ViewBag.laundry = model.Laundry;
            ViewBag.fridge = model.Fridge;
            ViewBag.cabinate = model.Cabinate;
            ViewBag.oven = model.Oven;
            ViewBag.windows = model.Windows;
            ViewBag.second_done = "yes";
            ViewBag.second_tab_done = "yes";
            //ViewBag.third_done = "yes";
            ViewBag.model_to_pass = item;
            ViewBag.Logged_user = HttpContext.Session.GetString("user");
            return View("Views/Helperland/Booknow.cshtml");
        }

        public IActionResult Booknow_third(Models.Book_now_Table model)
        {
            HttpContext.Session.SetString("Street", model.Street);
            HttpContext.Session.SetString("House_number", model.House_number);
            HttpContext.Session.SetString("Postal_code", model.Zipcode);
            HttpContext.Session.SetString("Location", model.Location);

            if((model.to_check).Equals(1))
            {
                HttpContext.Session.SetString("Phone_number", model.Phone);
            }

            var str = HttpContext.Session.GetString("user");
            _helperlandcontext = new HelperlandContext();
            var p = _helperlandcontext.Users.Where(x => x.FirstName == str).ToList();
            var user = p.FirstOrDefault().UserId;

            Models.UserAddress obj = new Models.UserAddress();
            obj.AddressLine1 = model.Street;
            obj.AddressLine2 = model.House_number;
            obj.City = model.Location;
            obj.PostalCode = model.Zipcode;
            obj.Mobile = model.Phone;
            obj.UserId = user;
            obj.IsDefault = false;
            obj.IsDeleted = false;


            
            _helperlandcontext.UserAddresses.Add(obj);
            _helperlandcontext.SaveChanges();

            var zcode = HttpContext.Session.GetString("zipcode");
            var model_to_pass = _helperlandcontext.UserAddresses.Where(x => x.UserId == user && x.PostalCode == zcode).ToList();

            List<Models.Book_now_Table> item = new List<Models.Book_now_Table>();
            foreach(Models.UserAddress temp in model_to_pass)
            {
                item.Add(new Models.Book_now_Table{Street=temp.AddressLine1, House_number=temp.AddressLine2,
                Location=temp.City, Zipcode=temp.PostalCode, Phone=temp.Mobile, ID=temp.AddressId});
            }

            ViewBag.main_hrs = float.Parse(HttpContext.Session.GetString("Booking_duration"));
            ViewBag.laundry = int.Parse(HttpContext.Session.GetString("Laundry"));
            ViewBag.fridge = int.Parse(HttpContext.Session.GetString("Fridge"));
            ViewBag.cabinate = int.Parse(HttpContext.Session.GetString("Cabinate"));
            ViewBag.oven = int.Parse(HttpContext.Session.GetString("Oven")); ;
            ViewBag.windows = int.Parse(HttpContext.Session.GetString("Windows"));
            ViewBag.second_done = "yes";
            //ViewBag.third_done = "yes";
            ViewBag.model_to_pass = item;
            ViewBag.Logged_user = HttpContext.Session.GetString("user");
            return View("Views/Helperland/Booknow.cshtml");
        }

        public IActionResult Booknow_select_address(Models.Book_now_Table model)
        {
            HttpContext.Session.SetString("Address_ID", (model.ID).ToString());
            ViewBag.main_hrs = float.Parse(HttpContext.Session.GetString("Booking_duration"));
            ViewBag.laundry = int.Parse(HttpContext.Session.GetString("Laundry"));
            ViewBag.fridge = int.Parse(HttpContext.Session.GetString("Fridge"));
            ViewBag.cabinate = int.Parse(HttpContext.Session.GetString("Cabinate"));
            ViewBag.oven = int.Parse(HttpContext.Session.GetString("Oven")); ;
            ViewBag.windows = int.Parse(HttpContext.Session.GetString("Windows"));
            ViewBag.third_done = "yes";
            ViewBag.second_done = "yes";
            ViewBag.third_tab_done = "yes";
            ViewBag.Logged_user = HttpContext.Session.GetString("user");

            return View("Views/Helperland/Booknow.cshtml");
        }

        public IActionResult Booknow_final(Models.Book_now_Table model)
        {

            _helperlandcontext = new HelperlandContext();
            var str = HttpContext.Session.GetString("user");
            var p = _helperlandcontext.Users.Where(x => x.FirstName == str).ToList();
            var user = p.FirstOrDefault().UserId;
            string str_date = HttpContext.Session.GetString("Booking_date");
            DateTime dt = DateTime.Parse(str_date);
            float str_dur = float.Parse(HttpContext.Session.GetString("Booking_duration"));
            float strt_time = float.Parse(HttpContext.Session.GetString("Booking_time"));
            string haspets = HttpContext.Session.GetString("Pets");
            bool pts;
            if (haspets == "1")
                pts = true;
            else pts = false;


            Models.ServiceRequest obj = new Models.ServiceRequest();
            obj.UserId = user;
            obj.ServiceId = 0;
            obj.ServiceStartDate = dt;
            obj.ZipCode = HttpContext.Session.GetString("zipcode");
            obj.ServiceHours = strt_time;
            obj.ExtraHours = str_dur;
            obj.SubTotal = Convert.ToDecimal(model.Discounted_cost);
            obj.TotalCost = Convert.ToDecimal(model.Total_cost);
            obj.HasPets = pts;
            obj.CreatedDate = DateTime.Now;
            obj.ModifiedDate = DateTime.Now;
            obj.Distance = 0;
            if(HttpContext.Session.GetString("Suggestion") != null)
            {
                obj.Comments = HttpContext.Session.GetString("Suggestion");
            }

            _helperlandcontext.ServiceRequests.Add(obj);
            _helperlandcontext.SaveChanges();

            int id_val = 0;
            var all_rows = _helperlandcontext.ServiceRequests.Where(x => true).ToList();

            foreach (Models.ServiceRequest item in all_rows)
            {
                id_val = item.ServiceRequestId;
            }

            int add_id =  int.Parse(HttpContext.Session.GetString("Address_ID"));

            var address_row = _helperlandcontext.UserAddresses.Where(x => x.AddressId == add_id).ToList();


            Models.ServiceRequestAddress address = new Models.ServiceRequestAddress();
            address.AddressLine1 = address_row.FirstOrDefault().AddressLine1;
            address.AddressLine2 = address_row.FirstOrDefault().AddressLine2;
            address.City = address_row.FirstOrDefault().City;
            address.PostalCode = address_row.FirstOrDefault().PostalCode;
            address.Mobile = address_row.FirstOrDefault().Mobile;
            address.ServiceRequestId = id_val;

            _helperlandcontext.ServiceRequestAddresses.Add(address);
            _helperlandcontext.SaveChanges();

           if(int.Parse(HttpContext.Session.GetString("Laundry")) == 1)
           {
                Models.ServiceRequestExtra extraa_sr = new Models.ServiceRequestExtra();
                extraa_sr.ServiceRequestId = id_val;
                extraa_sr.ServiceExtraId = 1;
                _helperlandcontext.ServiceRequestExtras.Add(extraa_sr);
                _helperlandcontext.SaveChanges();
            }
            if (int.Parse(HttpContext.Session.GetString("Fridge")) == 1)
            {
                Models.ServiceRequestExtra extraa_sr = new Models.ServiceRequestExtra();
                extraa_sr.ServiceRequestId = id_val;
                extraa_sr.ServiceExtraId = 2;
                _helperlandcontext.ServiceRequestExtras.Add(extraa_sr);
                _helperlandcontext.SaveChanges();
            }
            if (int.Parse(HttpContext.Session.GetString("Cabinate")) == 1)
            {
                Models.ServiceRequestExtra extraa_sr = new Models.ServiceRequestExtra();
                extraa_sr.ServiceRequestId = id_val;
                extraa_sr.ServiceExtraId = 3;
                _helperlandcontext.ServiceRequestExtras.Add(extraa_sr);
                _helperlandcontext.SaveChanges();
            }
            if (int.Parse(HttpContext.Session.GetString("Oven")) == 1)
            {
                Models.ServiceRequestExtra extraa_sr = new Models.ServiceRequestExtra();
                extraa_sr.ServiceRequestId = id_val;
                extraa_sr.ServiceExtraId = 4;
                _helperlandcontext.ServiceRequestExtras.Add(extraa_sr);
                _helperlandcontext.SaveChanges();
            }
            if (int.Parse(HttpContext.Session.GetString("Windows")) == 1)
            {
                Models.ServiceRequestExtra extraa_sr = new Models.ServiceRequestExtra();
                extraa_sr.ServiceRequestId = id_val;
                extraa_sr.ServiceExtraId = 5;
                _helperlandcontext.ServiceRequestExtras.Add(extraa_sr);
                _helperlandcontext.SaveChanges();
            }

            var z_code = HttpContext.Session.GetString("zipcode");

            var sp_s = _helperlandcontext.Users.Where(x => x.ZipCode == z_code && x.UserTypeId == 2).ToList();

            foreach (Models.User user_temp in sp_s)
            {
                MailMessage mm = new MailMessage("pmmakadiya1@gmail.com", user_temp.Email);
                mm.Subject = "New Service request.";
                mm.Body = "New service request is booked in your area, Please check if you are interested!";
                mm.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                NetworkCredential nc = new NetworkCredential("pmmakadiya1@gmail.com", "123456789@gmail.com");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = nc;
                smtp.Send(mm);
                //return View();
            }

            ViewBag.user = HttpContext.Session.GetString("user");
            ViewBag.final_done = "yes";
            return View("~/Views/Helperland/Register.cshtml"); 
        }



            public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
