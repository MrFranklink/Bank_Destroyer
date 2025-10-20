using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class LoanAccountRepository
    {
        /// <summary>
        /// Create a new loan account
        /// </summary>
        public bool CreateLoanAccount(string lnAccountId, string customerId, decimal loanAmount, DateTime startDate, int tenure, decimal lnRoi, decimal emi)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var newLoanAccount = new LoanAccount
                    {
                        Ln_accountid = lnAccountId,
                        Customer = customerId,
                        loan_amount = loanAmount,
                        Start_date = startDate,
                        Tenure = tenure,
                        Ln_roi = lnRoi,
                        Emi = emi,
                        Account = null,  // Explicitly set navigation properties to null
                        Customer1 = null
                    };

                    context.LoanAccounts.Add(newLoanAccount);
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
        /// Get loan account by ID
        /// </summary>
        public LoanAccount GetLoanAccountById(string lnAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.LoanAccounts.Find(lnAccountId);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all loan accounts by customer
        /// </summary>
        public List<LoanAccount> GetLoanAccountsByCustomerId(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.LoanAccounts
                        .Where(ln => ln.Customer == customerId)
                        .ToList();
                }
            }
            catch
            {
                return new List<LoanAccount>();
            }
        }

        /// <summary>
        /// Get all loan accounts
        /// </summary>
        public List<LoanAccount> GetAllLoanAccounts()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.LoanAccounts.ToList();
                }
            }
            catch
            {
                return new List<LoanAccount>();
            }
        }
    }
}
