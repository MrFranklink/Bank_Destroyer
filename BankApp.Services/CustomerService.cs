using System;
using System.Collections.Generic;
using System.Linq;
using DB;
using DB.Utilities;

namespace BankApp.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _customerRepo;
        private readonly UserLoginRepository _userLoginRepo;

        public CustomerService()
        {
            _customerRepo = new CustomerRepository();
            _userLoginRepo = new UserLoginRepository();
        }

        /// <summary>
        /// Register a new customer with auto-generated ID and UserLogin
        /// </summary>
        public OperationResult RegisterCustomer(string custName, DateTime dob, string pan, string address, string phoneNumber)
        {
            var validationRules = new List<Func<OperationResult>>
            {
                () => string.IsNullOrWhiteSpace(custName) ? Error("Customer Name is required") : null,
                () => string.IsNullOrWhiteSpace(pan) ? Error("PAN is required") : null,
                () => !IdGenerator.ValidatePanFormat(pan) ? Error("PAN must be 4 letters followed by 4 digits (e.g., ABCD1234)") : null,
                () => dob > DateTime.Now.AddYears(-18) ? Error("Customer must be at least 18 years old") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            try
            {
                // Auto-generate Customer ID
                string custId = IdGenerator.GenerateCustomerId();

                // Create customer
                bool customerCreated = _customerRepo.CreateCustomer(custId, custName, dob, pan, address, phoneNumber);
                
                if (!customerCreated)
                {
                    return Error("Failed to register customer. Please try again.");
                }

                // Auto-generate username from first name
                string username = IdGenerator.GenerateUsername(custName.Split(' ')[0]);

                // Create UserLogin with default password
                string defaultPassword = "Dummy";
                bool loginCreated = _userLoginRepo.CreateUser(custId, username, defaultPassword, "CUSTOMER", custId);

                if (!loginCreated)
                {
                    // Rollback: Delete the customer if login creation failed
                    // Note: In production, use transactions
                    return Error("Customer created but login failed. Please contact administrator.");
                }

                return Success($"Customer registered successfully! Customer ID: {custId}, Username: {username}, Password: {defaultPassword}", custId, username, defaultPassword);
            }
            catch (Exception ex)
            {
                return Error($"Registration failed: {ex.Message}");
            }
        }

        public List<CustomerDTO> GetAllCustomers()
        {
            var customers = _customerRepo.GetAllCustomers();
            return customers.Select(c => new CustomerDTO
            {
                Custid = c.Custid,
                Custname = c.Custname,
                DOB = c.DOB,
                Pan = c.Pan,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber
            }).ToList();
        }

        public int GetCustomerCount()
        {
            return _customerRepo.GetCustomerCount();
        }

        private OperationResult Error(string message) => new OperationResult { IsSuccess = false, Message = message };
        
        private OperationResult Success(string message, string custId = null, string username = null, string password = null) => 
            new OperationResult 
            { 
                IsSuccess = true, 
                Message = message,
                GeneratedId = custId,
                GeneratedUsername = username,
                GeneratedPassword = password
            };
    }

    // DTO to avoid exposing DB entities
    public class CustomerDTO
    {
        public string Custid { get; set; }
        public string Custname { get; set; }
        public DateTime? DOB { get; set; }
        public string Pan { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string GeneratedId { get; set; }
        public string GeneratedUsername { get; set; }
        public string GeneratedPassword { get; set; }
    }
}
