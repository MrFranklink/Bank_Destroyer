using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class ManagerRepository
    {
        public bool CreateManager(string managerId, string managerName, string pan)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    // Check if manager already exists
                    if (context.Managers.Any(m => m.ManagerID == managerId))
                    {
                        return false;
                    }

                    var newManager = new Manager
                    {
                        ManagerID = managerId,
                        ManagerName = managerName,
                        PAN = pan
                    };

                    context.Managers.Add(newManager);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Manager> GetAllManagers()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.ToList();
                }
            }
            catch
            {
                return new List<Manager>();
            }
        }

        public Manager GetManagerById(string managerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.FirstOrDefault(m => m.ManagerID == managerId);
                }
            }
            catch
            {
                return null;
            }
        }

        public bool ManagerExists(string managerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.Any(m => m.ManagerID == managerId);
                }
            }
            catch
            {
                return false;
            }
        }

        public int GetManagerCount()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.Count();
                }
            }
            catch
            {
                return 0;
            }
        }

        public bool DeleteManager(string managerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var manager = context.Managers.Find(managerId);
                    if (manager == null)
                    {
                        return false;
                    }

                    context.Managers.Remove(manager);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Delete a customer (cascade will delete UserLogin via trigger)
        /// </summary>
        /// <param name="customerId">Customer ID to delete</param>
        /// <returns>Operation result</returns>
        public DeleteOperationResult DeleteCustomer(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var customer = context.Customers.Find(customerId);
                    if (customer == null)
                    {
                        return new DeleteOperationResult
                        {
                            IsSuccess = false,
                            Message = $"Customer {customerId} not found"
                        };
                    }

                    // Check if customer has any open accounts
                    bool hasOpenAccounts = context.Accounts
                        .Any(a => a.CustomerID == customerId && a.Status == "OPEN");

                    if (hasOpenAccounts)
                    {
                        return new DeleteOperationResult
                        {
                            IsSuccess = false,
                            Message = $"Cannot delete customer {customerId}. Customer has open accounts. Please close all accounts first."
                        };
                    }

                    context.Customers.Remove(customer);
                    context.SaveChanges();

                    return new DeleteOperationResult
                    {
                        IsSuccess = true,
                        Message = $"Customer {customerId} deleted successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new DeleteOperationResult
                {
                    IsSuccess = false,
                    Message = $"Failed to delete customer: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete an employee (cascade will delete UserLogin via trigger)
        /// </summary>
        /// <param name="employeeId">Employee ID to delete</param>
        /// <returns>Operation result</returns>
        public DeleteOperationResult DeleteEmployee(string employeeId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var employee = context.Employees.Find(employeeId);
                    if (employee == null)
                    {
                        return new DeleteOperationResult
                        {
                            IsSuccess = false,
                            Message = $"Employee {employeeId} not found"
                        };
                    }

                    // Check if employee has opened any accounts that are still open
                    bool hasOpenedAccounts = context.Accounts
                        .Any(a => a.OpenedBy == employeeId && a.Status == "OPEN");

                    if (hasOpenedAccounts)
                    {
                        return new DeleteOperationResult
                        {
                            IsSuccess = false,
                            Message = $"Cannot delete employee {employeeId}. Employee has opened accounts that are still active. Please close those accounts first or reassign them."
                        };
                    }

                    context.Employees.Remove(employee);
                    context.SaveChanges();

                    return new DeleteOperationResult
                    {
                        IsSuccess = true,
                        Message = $"Employee {employeeId} deleted successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                return new DeleteOperationResult
                {
                    IsSuccess = false,
                    Message = $"Failed to delete employee: {ex.Message}"
                };
            }
        }
    }

    /// <summary>
    /// Result object for delete operations
    /// </summary>
    public class DeleteOperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
