using System;
using System.Collections.Generic;
using System.Linq;
using DB;
using DB.Utilities;

namespace BankApp.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _employeeRepo;
        private readonly UserLoginRepository _userLoginRepo;
        private readonly CustomerRepository _customerRepo;

        public EmployeeService()
        {
            _employeeRepo = new EmployeeRepository();
            _userLoginRepo = new UserLoginRepository();
            _customerRepo = new CustomerRepository();
        }

        /// <summary>
        /// Register a new employee with auto-generated ID and UserLogin
        /// Validates that employee cannot be a customer (PAN check)
        /// </summary>
        public OperationResult RegisterEmployee(string empName, string deptId, string pan, string loggedInUserPan = null)
        {
            // Clean and normalize inputs
            empName = empName?.Trim();
            deptId = deptId?.Trim().ToUpper();
            pan = pan?.ToUpper().Trim();
            loggedInUserPan = loggedInUserPan?.ToUpper().Trim();

            var validationRules = new List<Func<OperationResult>>
            {
                // Employee Name validations
                () => string.IsNullOrWhiteSpace(empName) ? Error("Employee Name is required") : null,
                () => empName.Length < 3 ? Error("Employee Name must be at least 3 characters long") : null,
                () => empName.Length > 50 ? Error("Employee Name cannot exceed 50 characters") : null,
                () => !System.Text.RegularExpressions.Regex.IsMatch(empName, @"^[a-zA-Z\s.]+$") 
                    ? Error("Employee Name can only contain letters, spaces, and dots (.)") : null,
                
                // Department validations
                () => string.IsNullOrWhiteSpace(deptId) ? Error("Department ID is required") : null,
                () => !new[] { "DEPT01", "DEPT02", "DEPT03" }.Contains(deptId) 
                    ? Error("Department ID must be DEPT01, DEPT02, or DEPT03") : null,
                
                // PAN validations
                () => string.IsNullOrWhiteSpace(pan) ? Error("PAN is required") : null,
                () => !IdGenerator.ValidatePanFormat(pan) ? Error("PAN must be in format: 5 letters + 4 digits + 1 letter (e.g., ABCDE1234F)") : null,
                
                // Self-registration check (if logged-in user PAN is provided)
                () => !string.IsNullOrWhiteSpace(loggedInUserPan) && pan == loggedInUserPan 
                    ? Error("You cannot register yourself as an Employee. Please contact your Manager.") : null,
                
                // Check PAN uniqueness in Employee table
                () => _employeeRepo.PanExists(pan) ? Error($"PAN number '{pan}' is already registered with another Employee. Each PAN can only be used once.") : null,
                
                // Check if PAN exists in Customer table (prevent dual roles)
                () => _customerRepo.PanExists(pan) ? Error($"PAN number '{pan}' is already registered as a Customer. Same person cannot be both Employee and Customer.") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            try
            {
                // Auto-generate Employee ID
                string empId = IdGenerator.GenerateEmployeeId();

                // Create employee
                bool employeeCreated = _employeeRepo.CreateEmployee(empId, empName, deptId, pan);
                
                if (!employeeCreated)
                {
                    return Error("Failed to register employee. Employee ID may already exist. Please try again.");
                }

                // Auto-generate username from first name
                string username = IdGenerator.GenerateUsername(empName.Split(' ')[0]);

                // Generate UNIQUE UserID for login (different from Employee ID)
                string userId = IdGenerator.GenerateUserId();

                // Create UserLogin with default password
                string defaultPassword = "Dummy";
                // UserID = unique login ID, ReferenceID = Employee ID
                bool loginCreated = _userLoginRepo.CreateUser(userId, username, defaultPassword, "EMPLOYEE", empId);

                if (!loginCreated)
                {
                    // Rollback: Delete the employee if login creation failed
                    // Note: In production, use transactions
                    return Error("Employee created but login failed. Please contact administrator.");
                }

                return Success($"Employee registered successfully! Employee ID: {empId}, Username: {username}, Password: {defaultPassword}, Department: {deptId}", empId, username, defaultPassword);
            }
            catch (Exception ex)
            {
                // Return detailed error message
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Details: " + ex.InnerException.Message;
                }
                return Error($"Registration failed: {errorMsg}");
            }
        }

        public List<EmployeeDTO> GetAllEmployees()
        {
            var employees = _employeeRepo.GetAllEmployees();
            return employees.Select(e => new EmployeeDTO
            {
                Empid = e.Empid,
                EmployeeName = e.EmployeeName,
                DeptId = e.DeptId,
                Pan = e.Pan
            }).ToList();
        }

        public EmployeeDTO GetEmployeeById(string empId)
        {
            var employee = _employeeRepo.GetEmployeeById(empId);
            if (employee == null)
                return null;

            return new EmployeeDTO
            {
                Empid = employee.Empid,
                EmployeeName = employee.EmployeeName,
                DeptId = employee.DeptId,
                Pan = employee.Pan
            };
        }

        public int GetEmployeeCount()
        {
            return _employeeRepo.GetEmployeeCount();
        }

        private OperationResult Error(string message) => new OperationResult { IsSuccess = false, Message = message };
        
        private OperationResult Success(string message, string empId = null, string username = null, string password = null) => 
            new OperationResult 
            { 
                IsSuccess = true, 
                Message = message,
                GeneratedId = empId,
                GeneratedUsername = username,
                GeneratedPassword = password
            };
    }

    // DTO to avoid exposing DB entities
    public class EmployeeDTO
    {
        public string Empid { get; set; }
        public string EmployeeName { get; set; }
        public string DeptId { get; set; }
        public string Pan { get; set; }
    }
}
