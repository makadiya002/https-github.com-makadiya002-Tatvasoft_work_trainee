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
    }
}
