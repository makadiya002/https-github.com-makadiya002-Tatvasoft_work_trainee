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

namespace Tatvasoft_Project.Controllers
{
    public class HelperlandController : Controller
    {
        private HelperlandContext _helperlandcontext;
        private object userManager;


        public object Session { get; set; }
    

        public IActionResult Index()
        {
            ViewBag.person = HttpContext.Session.GetString("user");
            return View();
        }

        public ViewResult Prices()
        {
            return View();
        }

        public ViewResult Contact()
        {
            return View();
        }

        public ViewResult FAQ()
        {
            return View();
        }

        public ViewResult About()
        {
            return View();
        }
        public ViewResult Become_helper()
        {
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
            string user = model.FirstName;
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
                        return View("~/Views/Helperland/Register.cshtml");
                    }
                    else if (p.FirstOrDefault().UserTypeId == 2)
                    {
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
            MailMessage mm = new MailMessage("pmmakadiya1@gmail.com", "princemakadiya4@gmail.com");
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

        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
