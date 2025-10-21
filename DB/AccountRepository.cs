using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class AccountRepository
    {
        /// <summary>
        /// Create a new account entry in the Account table
        /// </summary>
        public bool CreateAccount(string accountId, string accountType, string customerId, string openedBy, string openedByRole)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    // Debug logging
                    System.Diagnostics.Debug.WriteLine("=== CreateAccount Called ===");
                    System.Diagnostics.Debug.WriteLine($"AccountID: '{accountId}'");
                    System.Diagnostics.Debug.WriteLine($"AccountType: '{accountType}'");
                    System.Diagnostics.Debug.WriteLine($"CustomerID: '{customerId}'");
                    System.Diagnostics.Debug.WriteLine($"OpenedBy: '{openedBy}' (Length: {openedBy?.Length ?? 0})");
                    System.Diagnostics.Debug.WriteLine($"OpenedByRole: '{openedByRole}' (Length: {openedByRole?.Length ?? 0})");
                    
                    // Check if account already exists
                    if (context.Accounts.Any(a => a.AccountID == accountId))
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: Account already exists");
                        return false;
                    }

                    var newAccount = new Account
                    {
                        AccountID = accountId,
                        AccountType = accountType,
                        CustomerID = customerId,
                        OpenedBy = openedBy,
                        OpenedByRole = openedByRole,
                        OpenDate = DateTime.Now,
                        Status = "OPEN"
                    };

                    context.Accounts.Add(newAccount);
                    context.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine("SUCCESS: Account created");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"INNER: {ex.InnerException.Message}");
                }
                System.Diagnostics.Debug.WriteLine($"STACK: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Close an account
        /// </summary>
        public bool CloseAccount(string accountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var account = context.Accounts.Find(accountId);
                    if (account == null || account.Status == "CLOSED")
                    {
                        return false;
                    }

                    account.Status = "CLOSED";
                    account.ClosedDate = DateTime.Now;
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
        /// Get all accounts
        /// </summary>
        public List<Account> GetAllAccounts()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Accounts.ToList();
                }
            }
            catch
            {
                return new List<Account>();
            }
        }

        /// <summary>
        /// Get accounts by customer ID
        /// </summary>
        public List<Account> GetAccountsByCustomerId(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Accounts
                        .Where(a => a.CustomerID == customerId)
                        .ToList();
                }
            }
            catch
            {
                return new List<Account>();
            }
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        public Account GetAccountById(string accountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Accounts.Find(accountId);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if customer has account of specific type
        /// </summary>
        public bool CustomerHasAccountType(string customerId, string accountType)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Accounts.Any(a => a.CustomerID == customerId && a.AccountType == accountType && a.Status == "OPEN");
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get total account count
        /// </summary>
        public int GetTotalAccountCount()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Accounts.Count(a => a.Status == "OPEN");
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
