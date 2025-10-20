using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class FixedDepositAccountRepository
    {
        /// <summary>
        /// Create a new fixed deposit account
        /// </summary>
        public bool CreateFixedDepositAccount(string fdAccountId, string customerId, decimal amount, DateTime startDate, DateTime endDate, decimal fdRoi, decimal maturityAmount)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var newFdAccount = new FixedDepositAccount
                    {
                        FDAccountID = fdAccountId,
                        CustomerID = customerId,
                        Amount = amount,
                        StartDate = startDate,
                        EndDate = endDate,
                        FD_ROI = fdRoi,
                        MaturityAmount = maturityAmount,
                        Account = null,  // Explicitly set navigation properties to null
                        Customer = null
                    };

                    context.FixedDepositAccounts.Add(newFdAccount);
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
        /// Get FD account by ID
        /// </summary>
        public FixedDepositAccount GetFDAccountById(string fdAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.FixedDepositAccounts.Find(fdAccountId);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all FD accounts by customer
        /// </summary>
        public List<FixedDepositAccount> GetFDAccountsByCustomerId(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.FixedDepositAccounts
                        .Where(fd => fd.CustomerID == customerId)
                        .ToList();
                }
            }
            catch
            {
                return new List<FixedDepositAccount>();
            }
        }

        /// <summary>
        /// Get all FD accounts
        /// </summary>
        public List<FixedDepositAccount> GetAllFDAccounts()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.FixedDepositAccounts.ToList();
                }
            }
            catch
            {
                return new List<FixedDepositAccount>();
            }
        }
    }
}
