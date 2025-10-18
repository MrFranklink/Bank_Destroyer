using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class EmployeeRepository
    {
        public bool CreateEmployee(string empId, string empName, string deptId, string pan)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    // Check if employee already exists
                    if (context.Employees.Any(e => e.Empid == empId))
                    {
                        return false;
                    }

                    var newEmployee = new Employee
                    {
                        Empid = empId,
                        EmployeeName = empName,
                        DeptId = deptId,
                        Pan = pan
                    };

                    context.Employees.Add(newEmployee);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Employee> GetAllEmployees()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Employees.ToList();
                }
            }
            catch
            {
                return new List<Employee>();
            }
        }

        public Employee GetEmployeeById(string empId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Employees.FirstOrDefault(e => e.Empid == empId);
                }
            }
            catch
            {
                return null;
            }
        }

        public bool EmployeeExists(string empId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Employees.Any(e => e.Empid == empId);
                }
            }
            catch
            {
                return false;
            }
        }

        public int GetEmployeeCount()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Employees.Count();
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
