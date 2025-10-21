using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class SavingsAccountRepository
    {
        /// <summary>
        /// Create a new savings account
        /// </summary>
        public bool CreateSavingsAccount(string sbAccountId, string customerId, decimal initialBalance, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    System.Diagnostics.Debug.WriteLine("=== CreateSavingsAccount Called ===");
                    System.Diagnostics.Debug.WriteLine($"SBAccountID: '{sbAccountId}'");
                    System.Diagnostics.Debug.WriteLine($"CustomerID: '{customerId}'");
                    System.Diagnostics.Debug.WriteLine($"InitialBalance: {initialBalance}");

                    // Check if savings account already exists for this customer
                    if (context.SavingsAccounts.Any(sa => sa.Customerid == customerId))
                    {
                        errorMessage = "Customer already has a savings account";
                        System.Diagnostics.Debug.WriteLine("ERROR: Customer already has savings account");
                        return false;
                    }

                    var newSavingsAccount = new SavingsAccount
                    {
                        SBAccountID = sbAccountId,
                        Customerid = customerId,
                        Balance = initialBalance,
                        Account = null,  // Explicitly set navigation properties to null
                        Customer = null
                    };

                    System.Diagnostics.Debug.WriteLine("Adding SavingsAccount to context...");
                    context.SavingsAccounts.Add(newSavingsAccount);
                    
                    System.Diagnostics.Debug.WriteLine("Calling SaveChanges...");
                    context.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine("SUCCESS: SavingsAccount created");
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine($"INNER: {ex.InnerException.Message}");
                    
                    if (ex.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"INNER-INNER: {ex.InnerException.InnerException.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                return false;
            }
        }

        // Keep the old signature for backward compatibility
        public bool CreateSavingsAccount(string sbAccountId, string customerId, decimal initialBalance)
        {
            string errorMessage;
            return CreateSavingsAccount(sbAccountId, customerId, initialBalance, out errorMessage);
        }

        /// <summary>
        /// Get savings account by account ID
        /// </summary>
        public SavingsAccount GetSavingsAccountById(string sbAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsAccounts.Find(sbAccountId);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get savings account by customer ID
        /// </summary>
        public SavingsAccount GetSavingsAccountByCustomerId(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsAccounts.FirstOrDefault(sa => sa.Customerid == customerId);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Update account balance
        /// </summary>
        public bool UpdateBalance(string sbAccountId, decimal newBalance)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var account = context.SavingsAccounts.Find(sbAccountId);
                    if (account == null)
                    {
                        return false;
                    }

                    account.Balance = newBalance;
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
        /// Get all savings accounts
        /// </summary>
        public List<SavingsAccount> GetAllSavingsAccounts()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsAccounts.ToList();
                }
            }
            catch
            {
                return new List<SavingsAccount>();
            }
        }

        /// <summary>
        /// Check if customer already has a savings account
        /// </summary>
        public bool CustomerHasSavingsAccount(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsAccounts.Any(sa => sa.Customerid == customerId);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
