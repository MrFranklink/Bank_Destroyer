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
        private readonly SavingsTransactionService _transactionService;
        private readonly AccountManagementService _accountService;
        private readonly ManagerService _managerService;
        private readonly AuthService _authService;
        private readonly FundTransferService _fundTransferService;

        public DashboardController()
        {
            _customerService = new CustomerService();
            _employeeService = new EmployeeService();
            _savingsService = new SavingsAccountService();
            _fdService = new FixedDepositAccountService();
            _loanService = new LoanAccountService();
            _transactionService = new SavingsTransactionService();
            _accountService = new AccountManagementService();
            _managerService = new ManagerService();
            _authService = new AuthService();
            _fundTransferService = new FundTransferService();
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
                ViewBag.AccountCount = _accountService.GetTotalAccountCount();
                ViewBag.Accounts = _accountService.GetAllAccounts();
            }

            // For employee, load department info and customer list
            if (role.ToUpper() == "EMPLOYEE")
            {
                string deptId = Session["DeptId"]?.ToString() ?? "UNKNOWN";
                ViewBag.DeptId = deptId;
                
                // Load customer list (all employees can view customers)
                ViewBag.Customers = _customerService.GetAllCustomers();
                ViewBag.CustomerCount = _customerService.GetCustomerCount();
                
                // Load accounts based on department
                ViewBag.Accounts = _accountService.GetAllAccounts();
            }

            // For customer, load their personal data
            if (role.ToUpper() == "CUSTOMER")
            {
                string customerId = Session["ReferenceID"]?.ToString();
                if (!string.IsNullOrEmpty(customerId))
                {
                    // Load customer profile
                    var customerProfile = _accountService.GetCustomerProfile(customerId);
                    ViewBag.CustomerProfile = customerProfile;

                    // Load customer accounts
                    var accounts = _accountService.GetAccountsByCustomerId(customerId);
                    ViewBag.CustomerAccounts = accounts;

                    // Calculate statistics
                    var savingsAccount = accounts.FirstOrDefault(a => a.AccountType == "SAVING" && a.Status == "OPEN");
                    ViewBag.SavingsBalance = savingsAccount != null ? _accountService.GetSavingsBalance(savingsAccount.AccountID) : 0;
                    
                    ViewBag.TotalAccounts = accounts.Count(a => a.Status == "OPEN");
                    ViewBag.TotalFDAccounts = accounts.Count(a => a.AccountType == "FIXED-DEPOSIT" && a.Status == "OPEN");
                    ViewBag.TotalLoanAccounts = accounts.Count(a => a.AccountType == "LOAN" && a.Status == "OPEN");

                    // Get transaction count for savings account
                    if (savingsAccount != null)
                    {
                        var transactions = _transactionService.GetTransactionHistory(savingsAccount.AccountID);
                        ViewBag.TransactionCount = transactions?.Count ?? 0;
                    }
                    else
                    {
                        ViewBag.TransactionCount = 0;
                    }
                }
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
            string role = Session["Role"]?.ToString().ToUpper();
            string deptId = Session["DeptId"]?.ToString();

            // Check if user is manager or employee from DEPT01/DEPT02
            if (role != "MANAGER" && role != "EMPLOYEE")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Employees can only register customers if they are from DEPT01 or DEPT02
            if (role == "EMPLOYEE" && deptId != "DEPT01" && deptId != "DEPT02")
            {
                TempData["ErrorMessage"] = "Access denied. Only employees from Deposit Management (DEPT01) or Loan Management (DEPT02) can register customers.";
                return RedirectToAction("Index");
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
                TempData["ErrorMessage"] = "Access denied. Only Managers can register Employees.";
                return RedirectToAction("Index");
            }

            try
            {
                // Get logged-in Manager's ReferenceID (Manager ID)
                string loggedInManagerId = Session["ReferenceID"]?.ToString();
                string loggedInManagerPan = null;

                // Get Manager's PAN for self-registration check
                // Note: Currently Manager table doesn't track PAN like Employee/Customer
                // If needed, add PAN column to Manager table
                // For now, we skip self-check for Manager since they shouldn't register themselves
                
                var result = _employeeService.RegisterEmployee(empName, deptId, pan, loggedInManagerPan);

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
            string role = Session["Role"]?.ToString().ToUpper();
            string deptId = Session["DeptId"]?.ToString();

            // Check if user is manager or employee from DEPT01
            if (role != "MANAGER" && !(role == "EMPLOYEE" && deptId == "DEPT01"))
            {
                TempData["ErrorMessage"] = "Access denied. Only managers or Deposit Management (DEPT01) employees can open savings accounts.";
                return RedirectToAction("Index");
            }

            try
            {
                string openedBy = Session["ReferenceID"]?.ToString();
                string openedByRole = Session["Role"]?.ToString();

                // Store debug info in TempData for JavaScript access
                TempData["DebugInfo"] = new
                {
                    CustomerId = customerId,
                    InitialDeposit = initialDeposit,
                    OpenedBy = openedBy ?? "NULL",
                    OpenedByRole = openedByRole ?? "NULL",
                    SessionUserID = Session["UserID"]?.ToString() ?? "NULL",
                    SessionUserName = Session["UserName"]?.ToString() ?? "NULL"
                };

                // Check if ReferenceID is null or empty
                if (string.IsNullOrWhiteSpace(openedBy))
                {
                    TempData["ErrorMessage"] = $"{role} ID (ReferenceID) is NULL. Please log out and log in again, or update your UserLogin.ReferenceID in the database.";
                    TempData["ErrorType"] = "ReferenceIDNull";
                    return RedirectToAction("Index");
                }

                var result = _savingsService.OpenSavingsAccount(customerId, initialDeposit, openedBy, openedByRole);

                TempData["ServiceResult"] = new
                {
                    IsSuccess = result.IsSuccess,
                    Message = result.Message,
                    AccountId = result.AccountId,
                    Balance = result.Balance
                };

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    TempData["ErrorType"] = "ServiceError";
                }
            }
            catch (Exception ex)
            {
                var errorDetails = new
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    InnerStackTrace = ex.InnerException?.StackTrace,
                    Source = ex.Source
                };

                TempData["ExceptionDetails"] = errorDetails;
                TempData["ErrorMessage"] = "Failed to open savings account: " + ex.Message;
                TempData["ErrorType"] = "Exception";
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/OpenFixedDepositAccount
        [HttpPost]
        public ActionResult OpenFixedDepositAccount(string customerId, decimal amount, DateTime startDate, int tenureMonths)
        {
            string role = Session["Role"]?.ToString().ToUpper();
            string deptId = Session["DeptId"]?.ToString();

            // Check if user is manager or employee from DEPT01
            if (role != "MANAGER" && !(role == "EMPLOYEE" && deptId == "DEPT01"))
            {
                TempData["ErrorMessage"] = "Access denied. Only managers or Deposit Management (DEPT01) employees can open fixed deposit accounts.";
                return RedirectToAction("Index");
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
            string role = Session["Role"]?.ToString().ToUpper();
            string deptId = Session["DeptId"]?.ToString();

            // Check if user is manager or employee from DEPT02
            if (role != "MANAGER" && !(role == "EMPLOYEE" && deptId == "DEPT02"))
            {
                TempData["ErrorMessage"] = "Access denied. Only managers or Loan Management (DEPT02) employees can open loan accounts.";
                return RedirectToAction("Index");
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

        // POST: Dashboard/Deposit
        [HttpPost]
        public ActionResult Deposit(string accountId, decimal amount)
        {
            string role = Session["Role"]?.ToString().ToUpper();
            string deptId = Session["DeptId"]?.ToString();

            // Check if user is manager or employee from DEPT01
            if (role != "MANAGER" && !(role == "EMPLOYEE" && deptId == "DEPT01"))
            {
                TempData["ErrorMessage"] = "Access denied. Only managers or Deposit Management (DEPT01) employees can process deposits.";
                return RedirectToAction("Index");
            }

            try
            {
                var result = _transactionService.Deposit(accountId, amount);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message + $" New Balance: Rs. {result.NewBalance:N2}";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Deposit failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/Withdraw
        [HttpPost]
        public ActionResult Withdraw(string accountId, decimal amount)
        {
            // Only managers can withdraw
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                TempData["ErrorMessage"] = "Access denied. Only managers can process withdrawals.";
                return RedirectToAction("Index");
            }

            try
            {
                var result = _transactionService.Withdraw(accountId, amount);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message + $" New Balance: Rs. {result.NewBalance:N2}";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Withdrawal failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/CloseAccount
        [HttpPost]
        public ActionResult CloseAccount(string accountId, string accountType)
        {
            string role = Session["Role"]?.ToString().ToUpper();
            string deptId = Session["DeptId"]?.ToString();

            // Determine if user has permission based on account type
            bool hasPermission = false;
            
            if (role == "MANAGER")
            {
                hasPermission = true;
            }
            else if (role == "EMPLOYEE")
            {
                // DEPT01 can close Savings and FD accounts
                if (deptId == "DEPT01" && (accountType == "SAVING" || accountType == "FIXED-DEPOSIT"))
                {
                    hasPermission = true;
                }
                // DEPT02 can close Loan accounts
                else if (deptId == "DEPT02" && accountType == "LOAN")
                {
                    hasPermission = true;
                }
            }

            if (!hasPermission)
            {
                TempData["ErrorMessage"] = "Access denied. You don't have permission to close this account type.";
                return RedirectToAction("Index");
            }

            try
            {
                AccountOperationResult result = null;

                if (accountType == "SAVING")
                {
                    result = _savingsService.CloseSavingsAccount(accountId);
                }
                else if (accountType == "FIXED-DEPOSIT")
                {
                    result = _fdService.ForeCloseFDAccount(accountId);
                }
                else if (accountType == "LOAN")
                {
                    result = _loanService.ForeCloseLoanAccount(accountId);
                }

                if (result != null && result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Message ?? "Failed to close account";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to close account: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/DeleteCustomer
        [HttpPost]
        public ActionResult DeleteCustomer(string custId)
        {
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = _managerService.DeleteCustomer(custId);

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
                TempData["ErrorMessage"] = "Failed to delete customer: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/DeleteEmployee
        [HttpPost]
        public ActionResult DeleteEmployee(string empId)
        {
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = _managerService.DeleteEmployee(empId);

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
                TempData["ErrorMessage"] = "Failed to delete employee: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: Dashboard/GetTransactionHistory
        [HttpGet]
        public ActionResult GetTransactionHistory(string accountId)
        {
            if (Session["Role"]?.ToString().ToUpper() != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var transactions = _transactionService.GetTransactionHistory(accountId);
                return Json(new { success = true, data = transactions }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Dashboard/GetCustomerTransactionHistory
        [HttpGet]
        public ActionResult GetCustomerTransactionHistory(string accountId)
        {
            // Allow both CUSTOMER and MANAGER to access
            string role = Session["Role"]?.ToString().ToUpper();
            if (role != "CUSTOMER" && role != "MANAGER")
            {
                return RedirectToAction("Login", "Auth");
            }

            // If customer, verify they own this account
            if (role == "CUSTOMER")
            {
                string customerId = Session["ReferenceID"]?.ToString();
                var account = _accountService.GetAccountById(accountId);
                
                if (account == null || account.CustomerID != customerId)
                {
                    return Json(new { success = false, message = "Unauthorized access" }, JsonRequestBehavior.AllowGet);
                }
            }

            try
            {
                var transactions = _transactionService.GetTransactionHistory(accountId);
                return Json(new { success = true, data = transactions }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Dashboard/CustomerDeposit - Allow customers to deposit to their own savings account
        [HttpPost]
        public ActionResult CustomerDeposit(decimal amount)
        {
            if (Session["Role"]?.ToString().ToUpper() != "CUSTOMER")
            {
                TempData["ErrorMessage"] = "Access denied. Only customers can use this feature.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string customerId = Session["ReferenceID"]?.ToString();
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"=== CustomerDeposit Called ===");
                System.Diagnostics.Debug.WriteLine($"Customer ID: {customerId}");
                System.Diagnostics.Debug.WriteLine($"Amount: {amount}");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["ErrorMessage"] = "Customer ID not found in session. Please log out and log in again.";
                    return RedirectToAction("Index");
                }
                
                // Get customer's savings account
                var accounts = _accountService.GetAccountsByCustomerId(customerId);
                System.Diagnostics.Debug.WriteLine($"Found {accounts?.Count ?? 0} accounts");
                
                var savingsAccount = accounts?.FirstOrDefault(a => a.AccountType == "SAVING" && a.Status == "OPEN");

                if (savingsAccount == null)
                {
                    TempData["ErrorMessage"] = "You don't have an active savings account. Please contact the branch to open one.";
                    System.Diagnostics.Debug.WriteLine("ERROR: No active savings account found");
                    return RedirectToAction("Index");
                }

                System.Diagnostics.Debug.WriteLine($"Savings Account ID: {savingsAccount.AccountID}");

                var result = _transactionService.Deposit(savingsAccount.AccountID, amount);

                System.Diagnostics.Debug.WriteLine($"Transaction Result: {result.IsSuccess}");
                System.Diagnostics.Debug.WriteLine($"Message: {result.Message}");

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message + $" New Balance: ₹ {result.NewBalance:N2}";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = "Deposit failed: " + ex.Message;
                
                if (ex.InnerException != null)
                {
                    TempData["ErrorMessage"] += " | Inner: " + ex.InnerException.Message;
                }
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/CustomerWithdraw - Allow customers to withdraw from their own savings account
        [HttpPost]
        public ActionResult CustomerWithdraw(decimal amount)
        {
            if (Session["Role"]?.ToString().ToUpper() != "CUSTOMER")
            {
                TempData["ErrorMessage"] = "Access denied. Only customers can use this feature.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                string customerId = Session["ReferenceID"]?.ToString();
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"=== CustomerWithdraw Called ===");
                System.Diagnostics.Debug.WriteLine($"Customer ID: {customerId}");
                System.Diagnostics.Debug.WriteLine($"Amount: {amount}");
                
                if (string.IsNullOrEmpty(customerId))
                {
                    TempData["ErrorMessage"] = "Customer ID not found in session. Please log out and log in again.";
                    return RedirectToAction("Index");
                }
                
                // Get customer's savings account
                var accounts = _accountService.GetAccountsByCustomerId(customerId);
                System.Diagnostics.Debug.WriteLine($"Found {accounts?.Count ?? 0} accounts");
                
                var savingsAccount = accounts?.FirstOrDefault(a => a.AccountType == "SAVING" && a.Status == "OPEN");

                if (savingsAccount == null)
                {
                    TempData["ErrorMessage"] = "You don't have an active savings account.";
                    System.Diagnostics.Debug.WriteLine("ERROR: No active savings account found");
                    return RedirectToAction("Index");
                }

                System.Diagnostics.Debug.WriteLine($"Savings Account ID: {savingsAccount.AccountID}");

                var result = _transactionService.Withdraw(savingsAccount.AccountID, amount);

                System.Diagnostics.Debug.WriteLine($"Transaction Result: {result.IsSuccess}");
                System.Diagnostics.Debug.WriteLine($"Message: {result.Message}");

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message + $" New Balance: ₹ {result.NewBalance:N2}";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = "Withdrawal failed: " + ex.Message;
                
                if (ex.InnerException != null)
                {
                    TempData["ErrorMessage"] += " | Inner: " + ex.InnerException.Message;
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Dashboard/ChangePassword
        public ActionResult ChangePassword()
        {
            // Check if user is logged in
            if (Session["UserID"] == null || Session["Role"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.UserName = Session["UserName"];
            ViewBag.Role = Session["Role"];

            return View();
        }

        // POST: Dashboard/ChangePassword
        [HttpPost]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            // Get current user details from session
            string userId = Session["UserID"]?.ToString();

            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = _authService.ChangePassword(userId, oldPassword, newPassword, confirmNewPassword);

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
                TempData["ErrorMessage"] = "Failed to change password: " + ex.Message;
            }

            return RedirectToAction("ChangePassword");
        }

        // POST: Dashboard/TransferFunds
        [HttpPost]
        public ActionResult TransferFunds(string toAccountId, decimal amount, string remarks)
        {
            // Check if user is customer
            if (Session["Role"]?.ToString().ToUpper() != "CUSTOMER")
            {
                TempData["ErrorMessage"] = "Only customers can transfer funds";
                return RedirectToAction("Index");
            }

            string customerId = Session["ReferenceID"]?.ToString();

            try
            {
                var result = _fundTransferService.TransferFunds(customerId, toAccountId, amount, remarks);

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
                TempData["ErrorMessage"] = "Transfer failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: Dashboard/PayLoanEMI
        [HttpPost]
        public ActionResult PayLoanEMI(string loanAccountId, decimal paymentAmount, string paymentType)
        {
            // Check if user is customer
            if (Session["Role"]?.ToString().ToUpper() != "CUSTOMER")
            {
                TempData["ErrorMessage"] = "Only customers can pay loan EMI";
                return RedirectToAction("Index");
            }

            string customerId = Session["ReferenceID"]?.ToString();

            try
            {
                var result = _loanService.PayEMI(loanAccountId, customerId, paymentAmount, paymentType);

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
                TempData["ErrorMessage"] = "Payment failed: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}