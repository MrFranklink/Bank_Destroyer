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
                    
                    // Log before save
                    System.Diagnostics.Debug.WriteLine($"=== Saving LoanAccount ===");
                    System.Diagnostics.Debug.WriteLine($"Ln_accountid: {lnAccountId}");
                    System.Diagnostics.Debug.WriteLine($"Customer: {customerId}");
                    System.Diagnostics.Debug.WriteLine($"loan_amount: {loanAmount}");
                    System.Diagnostics.Debug.WriteLine($"Start_date: {startDate}");
                    System.Diagnostics.Debug.WriteLine($"Tenure: {tenure}");
                    System.Diagnostics.Debug.WriteLine($"Ln_roi: {lnRoi}");
                    System.Diagnostics.Debug.WriteLine($"Emi: {emi}");
                    
                    context.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine("SUCCESS: LoanAccount saved");
                    return true;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException validationEx)
            {
                System.Diagnostics.Debug.WriteLine("=== Entity Validation Error in LoanAccount ===");
                foreach (var validationErrors in validationEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
                throw;
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException dbEx)
            {
                System.Diagnostics.Debug.WriteLine("=== Database Update Error in LoanAccount ===");
                System.Diagnostics.Debug.WriteLine($"Error: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner: {dbEx.InnerException.Message}");
                    if (dbEx.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Inner: {dbEx.InnerException.InnerException.Message}");
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== General Error in CreateLoanAccount ===");
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
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
