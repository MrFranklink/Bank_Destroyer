using System;
using DB;

namespace BankApp.Services
{
    /// <summary>
    /// Service for manager-specific operations (delete staff)
    /// </summary>
    public class ManagerService
    {
        private readonly ManagerRepository _managerRepo;

        public ManagerService()
        {
            _managerRepo = new ManagerRepository();
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        public OperationResult DeleteCustomer(string customerId)
        {
            var result = _managerRepo.DeleteCustomer(customerId);
            
            return new OperationResult
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message
            };
        }

        /// <summary>
        /// Delete an employee
        /// </summary>
        public OperationResult DeleteEmployee(string employeeId)
        {
            var result = _managerRepo.DeleteEmployee(employeeId);
            
            return new OperationResult
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message
            };
        }
    }
}
