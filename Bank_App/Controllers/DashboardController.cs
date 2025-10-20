using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BankApp.Services;

namespace Bank_App.Controllers
{
    public class DashboardController : Controller
    {
        private readonly CustomerService _customerService;
        private readonly EmployeeService _employeeService;
        private readonly SavingsAccountService _savingsService;
        private readonly FixedDepositAccountService _fdService;
        private readonly LoanAccountService _loanService;

        public DashboardController()
        {
            _customerService = new CustomerService();
            _employeeService = new EmployeeService();
            _savingsService = new SavingsAccountService();
            _fdService = new FixedDepositAccountService();
            _loanService = new LoanAccountService();
        }

        // GET: Dashboard/Index
        public ActionResult Index()
        {
            // Check if user is logged in
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string role = Session["Role"].ToString();
            ViewBag.UserName = Session["UserName"];
            ViewBag.UserID = Session["UserID"];
            ViewBag.ReferenceID = Session["ReferenceID"];
            ViewBag.Role = role;

            // For manager, load customer and employee lists and counts
            if (role.ToUpper() == "MANAGER")
            {
                ViewBag.Customers = _customerService.GetAllCustomers();
                ViewBag.Employees = _employeeService.GetAllEmployees();
                ViewBag.CustomerCount = _customerService.GetCustomerCount();
                ViewBag.EmployeeCount = _employeeService.GetEmployeeCount();
            }

            // Return different views based on role (case-insensitive)
            switch (role.ToUpper())
            {
                case "CUSTOMER":
                    return View("CustomerDashboard");
                case "EMPLOYEE":
                    return View("EmployeeDashboard");
                case "MANAGER":
                    return View("ManagerDashboard");
                default:
                    return RedirectToAction("Login", "Auth");
            }
        }

        // POST: Dashboard/RegisterCustomer
        [HttpPost]
        public ActionResult RegisterCustomer(string custName, DateTime dob, string pan, string phone, string address)
        {
            // Check if user is manager
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = _customerService.RegisterCustomer(custName, dob, pan, address, phone);

                if (result.IsSuccess)
                {
                    // Display credentials in success message
                    TempData["SuccessMessage"] = $"{result.Message}";
                    TempData["ShowCredentials"] = true;
                    TempData["CredentialsTitle"] = "Customer Login Credentials";
                    TempData["CredentialsId"] = result.GeneratedId;
                    TempData["CredentialsUsername"] = result.GeneratedUsername;
                    TempData["CredentialsPassword"] = result.GeneratedPassword;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to register customer: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/RegisterEmployee
        [HttpPost]
        public ActionResult RegisterEmployee(string empName, string deptId, string pan)
        {
            // Check if user is manager
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = _employeeService.RegisterEmployee(empName, deptId, pan);

                if (result.IsSuccess)
                {
                    // Display credentials in success message
                    TempData["SuccessMessage"] = $"{result.Message}";
                    TempData["ShowCredentials"] = true;
                    TempData["CredentialsTitle"] = "Employee Login Credentials";
                    TempData["CredentialsId"] = result.GeneratedId;
                    TempData["CredentialsUsername"] = result.GeneratedUsername;
                    TempData["CredentialsPassword"] = result.GeneratedPassword;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to register employee: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/OpenSavingsAccount
        [HttpPost]
        public ActionResult OpenSavingsAccount(string customerId, decimal initialDeposit)
        {
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string openedBy = Session["ReferenceID"]?.ToString();
                string openedByRole = Session["Role"]?.ToString();

                var result = _savingsService.OpenSavingsAccount(customerId, initialDeposit, openedBy, openedByRole);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to open savings account: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/OpenFixedDepositAccount
        [HttpPost]
        public ActionResult OpenFixedDepositAccount(string customerId, decimal amount, DateTime startDate, int tenureMonths)
        {
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string openedBy = Session["ReferenceID"]?.ToString();
                string openedByRole = Session["Role"]?.ToString();

                var result = _fdService.OpenFixedDepositAccount(customerId, amount, startDate, tenureMonths, openedBy, openedByRole);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to open fixed deposit: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/OpenLoanAccount
        [HttpPost]
        public ActionResult OpenLoanAccount(string customerId, decimal loanAmount, DateTime startDate, int tenureMonths, decimal monthlySalary)
        {
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string openedBy = Session["ReferenceID"]?.ToString();
                string openedByRole = Session["Role"]?.ToString();

                var result = _loanService.OpenLoanAccount(customerId, loanAmount, startDate, tenureMonths, monthlySalary, openedBy, openedByRole);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to open loan account: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}

      