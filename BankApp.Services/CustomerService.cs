using System;
using System.Collections.Generic;
using System.Linq;
using DB;

namespace BankApp.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _customerRepo;

        public CustomerService()
        {
            _customerRepo = new CustomerRepository();
        }

        public OperationResult RegisterCustomer(string custId, string custName, DateTime dob, string pan, string address, string phoneNumber)
        {
            var validationRules = new List<Func<OperationResult>>
            {
                () => string.IsNullOrWhiteSpace(custId) ? Error("Customer ID is required") : null,
                () => string.IsNullOrWhiteSpace(custName) ? Error("Customer Name is required") : null,
                () => string.IsNullOrWhiteSpace(pan) ? Error("PAN is required") : null,
                () => dob > DateTime.Now.AddYears(-18) ? Error("Customer must be at least 18 years old") : null,
                () => _customerRepo.CustomerExists(custId) ? Error("Customer ID already exists") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            bool success = _customerRepo.CreateCustomer(custId, custName, dob, pan, address, phoneNumber);
            return success ? Success("Customer registered successfully!") : Error("Failed to register customer. Please try again.");
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
        private OperationResult Success(string message) => new OperationResult { IsSuccess = true, Message = message };
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
    }
}
