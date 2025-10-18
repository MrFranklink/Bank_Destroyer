using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BankApp.Services;

namespace Bank_App.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController()
        {
            _authService = new AuthService();
        }

        // GET: Auth/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        public ActionResult Login(string id, string password)
        {
            var result = _authService.ValidateLogin(id, password);

            if (result.IsSuccess)
            {
                Session["UserID"] = result.UserID;
                Session["UserName"] = result.UserName;
                Session["Role"] = result.Role;
                Session["ReferenceID"] = result.ReferenceID;

                FormsAuthentication.SetAuthCookie(result.UserName, false);

                // Redirect to single Dashboard controller
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = result.Message;
            return View();
        }

        // GET: Auth/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        public ActionResult Register(string userId, string userName, string password, string confirmPassword, string role, string referenceId)
        {
            var result = _authService.RegisterUser(userId, userName, password, confirmPassword, role, referenceId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction("Login");
            }

            ViewBag.Error = result.Message;
            return View();
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}