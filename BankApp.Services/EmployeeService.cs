using System;
using System.Collections.Generic;
using System.Linq;
using DB;

namespace BankApp.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _employeeRepo;

        public EmployeeService()
        {
            _employeeRepo = new EmployeeRepository();
        }

        public OperationResult RegisterEmployee(string empId, string empName, string deptId, string pan)
        {
            var validationRules = new List<Func<OperationResult>>
            {
                () => string.IsNullOrWhiteSpace(empId) ? Error("Employee ID is required") : null,
                () => string.IsNullOrWhiteSpace(empName) ? Error("Employee Name is required") : null,
                () => string.IsNullOrWhiteSpace(deptId) ? Error("Department ID is required") : null,
                () => string.IsNullOrWhiteSpace(pan) ? Error("PAN is required") : null,
                () => !empId.StartsWith("26") ? Error("Employee ID must start with '26'") : null,
                () => _employeeRepo.EmployeeExists(empId) ? Error("Employee ID already exists") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            bool success = _employeeRepo.CreateEmployee(empId, empName, deptId, pan);
            return success ? Success("Employee registered successfully!") : Error("Failed to register employee. Please try again.");
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
        private OperationResult Success(string message) => new OperationResult { IsSuccess = true, Message = message };
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
