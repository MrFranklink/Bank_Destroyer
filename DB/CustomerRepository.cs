using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class CustomerRepository
    {
        public bool CreateCustomer(string custId, string custName, DateTime dob, string pan, string address, string phoneNumber)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    // Check if customer already exists
                    if (context.Customers.Any(c => c.Custid == custId))
                    {
                        return false;
                    }

                    var newCustomer = new Customer
                    {
                        Custid = custId,
                        Custname = custName,
                        DOB = dob,
                        Pan = pan,
                        Address = address,
                        PhoneNumber = phoneNumber
                    };

                    context.Customers.Add(newCustomer);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Customer> GetAllCustomers()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Customers.ToList();
                }
            }
            catch
            {
                return new List<Customer>();
            }
        }

        public Customer GetCustomerById(string custId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Customers.FirstOrDefault(c => c.Custid == custId);
                }
            }
            catch
            {
                return null;
            }
        }

        public bool CustomerExists(string custId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Customers.Any(c => c.Custid == custId);
                }
            }
            catch
            {
                return false;
            }
        }

        public int GetCustomerCount()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Customers.Count();
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if PAN number already exists in the system
        /// PAN should be unique across all customers
        /// </summary>
        /// <param name="pan">PAN number to check</param>
        /// <returns>True if PAN already exists</returns>
        public bool PanExists(string pan)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Customers.Any(c => c.Pan == pan);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get customer by PAN number
        /// </summary>
        /// <param name="pan">PAN number</param>
        /// <returns>Customer with matching PAN</returns>
        public Customer GetCustomerByPan(string pan)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Customers.FirstOrDefault(c => c.Pan == pan);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
