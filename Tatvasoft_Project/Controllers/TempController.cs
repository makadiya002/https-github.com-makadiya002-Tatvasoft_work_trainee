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
            var userid = _helperlandcontext.Users.Where(x => x.FirstName == username).ToList().FirstOrDefault().UserId;
            var all_ss = _helperlandcontext.ServiceRequests.Where(x => x.ServiceProviderId == userid).ToList();

            foreach(var temp in all_ss)
            {
                temp.Comments = temp.ServiceStartDate.ToString("yyyy-MM-dd");
            }

            ViewBag.all_ss = all_ss;
            return View();
        }
    }
}
