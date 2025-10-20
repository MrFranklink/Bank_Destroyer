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

        public EmployeeService()
        {
            _employeeRepo = new EmployeeRepository();
            _userLoginRepo = new UserLoginRepository();
        }

        /// <summary>
        /// Register a new employee with auto-generated ID and UserLogin
        /// </summary>
        public OperationResult RegisterEmployee(string empName, string deptId, string pan)
        {
            var validationRules = new List<Func<OperationResult>>
            {
                () => string.IsNullOrWhiteSpace(empName) ? Error("Employee Name is required") : null,
                () => string.IsNullOrWhiteSpace(deptId) ? Error("Department ID is required") : null,
                () => string.IsNullOrWhiteSpace(pan) ? Error("PAN is required") : null,
                () => !IdGenerator.ValidatePanFormat(pan) ? Error("PAN must be 4 letters followed by 4 digits (e.g., ABCD1234)") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            try
            {
                // Auto-generate Employee ID (starts with 26)
                string empId = IdGenerator.GenerateEmployeeId();

                // Create employee
                bool employeeCreated = _employeeRepo.CreateEmployee(empId, empName, deptId, pan);
                
                if (!employeeCreated)
                {
                    return Error("Failed to register employee. Please try again.");
                }

                // Auto-generate username from first name
                string username = IdGenerator.GenerateUsername(empName.Split(' ')[0]);

                // Create UserLogin with default password
                string defaultPassword = "Dummy";
                bool loginCreated = _userLoginRepo.CreateUser(empId, username, defaultPassword, "EMPLOYEE", empId);

                if (!loginCreated)
                {
                    // Rollback: Delete the employee if login creation failed
                    return Error("Employee created but login failed. Please contact administrator.");
                }

                return Success($"Employee registered successfully! Employee ID: {empId}, Username: {username}, Password: {defaultPassword}", empId, username, defaultPassword);
            }
            catch (Exception ex)
            {
                return Error($"Registration failed: {ex.Message}");
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
