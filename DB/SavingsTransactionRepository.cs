using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    /// <summary>
    /// Repository for managing savings account transactions
    /// </summary>
    public class SavingsTransactionRepository
    {
        /// <summary>
        /// Record a transaction (deposit or withdrawal) in the SavingsTransaction table
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <param name="transactionType">DEPOSIT or WITHDRAW</param>
        /// <param name="amount">Transaction amount</param>
        /// <returns>True if transaction recorded successfully</returns>
        public bool RecordTransaction(string sbAccountId, string transactionType, decimal amount)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    // Disable Entity Framework validation to avoid navigation property issues
                    context.Configuration.ValidateOnSaveEnabled = false;
                    
                    var transaction = new SavingsTransaction
                    {
                        // DO NOT SET Transactionid - let database auto-generate it
                        SBAccountID = sbAccountId,
                        Transationdate = DateTime.Now,
                        Transactiontype = transactionType.ToUpper(),
                        Amount = amount,
                        SavingsAccount = null  // Explicitly set navigation property to null
                    };

                    System.Diagnostics.Debug.WriteLine($"=== Recording Transaction ===");
                    System.Diagnostics.Debug.WriteLine($"SBAccountID: {sbAccountId}");
                    System.Diagnostics.Debug.WriteLine($"Type: {transactionType}");
                    System.Diagnostics.Debug.WriteLine($"Amount: {amount}");

                    context.SavingsTransactions.Add(transaction);
                    context.SaveChanges();
                    
                    System.Diagnostics.Debug.WriteLine($"Transaction recorded successfully with ID: {transaction.Transactionid}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Enhanced error logging
                System.Diagnostics.Debug.WriteLine($"=== TRANSACTION ERROR ===");
                System.Diagnostics.Debug.WriteLine($"Error recording transaction: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    
                    if (ex.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Get all transactions for a specific savings account
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>List of transactions ordered by date (newest first)</returns>
        public List<SavingsTransaction> GetTransactionsByAccountId(string sbAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsTransactions
                        .Where(t => t.SBAccountID == sbAccountId)
                        .OrderByDescending(t => t.Transationdate)
                        .ToList();
                }
            }
            catch
            {
                return new List<SavingsTransaction>();
            }
        }

        /// <summary>
        /// Get transactions within a specific date range
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>List of transactions in date range</returns>
        public List<SavingsTransaction> GetTransactionsByDateRange(string sbAccountId, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsTransactions
                        .Where(t => t.SBAccountID == sbAccountId &&
                                    t.Transationdate >= startDate &&
                                    t.Transationdate <= endDate)
                        .OrderByDescending(t => t.Transationdate)
                        .ToList();
                }
            }
            catch
            {
                return new List<SavingsTransaction>();
            }
        }

        /// <summary>
        /// Get all transactions for all accounts (for reports)
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>List of all transactions</returns>
        public List<SavingsTransaction> GetAllTransactions(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var query = context.SavingsTransactions.AsQueryable();

                    if (startDate.HasValue)
                    {
                        query = query.Where(t => t.Transationdate >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(t => t.Transationdate <= endDate.Value);
                    }

                    return query.OrderByDescending(t => t.Transationdate).ToList();
                }
            }
            catch
            {
                return new List<SavingsTransaction>();
            }
        }

        /// <summary>
        /// Get total deposits for an account
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>Total deposit amount</returns>
        public decimal GetTotalDeposits(string sbAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsTransactions
                        .Where(t => t.SBAccountID == sbAccountId && t.Transactiontype == "DEPOSIT")
                        .Sum(t => t.Amount ?? 0);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get total withdrawals for an account
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>Total withdrawal amount</returns>
        public decimal GetTotalWithdrawals(string sbAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsTransactions
                        .Where(t => t.SBAccountID == sbAccountId && t.Transactiontype == "WITHDRAW")
                        .Sum(t => t.Amount ?? 0);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get transaction count for an account
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>Total number of transactions</returns>
        public int GetTransactionCount(string sbAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.SavingsTransactions
                        .Count(t => t.SBAccountID == sbAccountId);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get last transaction date for an account
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>Date of last transaction, or null if no transactions</returns>
        public DateTime? GetLastTransactionDate(string sbAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var lastTransaction = context.SavingsTransactions
                        .Where(t => t.SBAccountID == sbAccountId)
                        .OrderByDescending(t => t.Transationdate)
                        .FirstOrDefault();

                    return lastTransaction?.Transationdate;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get transaction statistics for today
        /// </summary>
        /// <returns>Count of today's transactions</returns>
        public int GetTodayTransactionCount()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    DateTime today = DateTime.Today;
                    DateTime tomorrow = today.AddDays(1);

                    return context.SavingsTransactions
                        .Count(t => t.Transationdate >= today && t.Transationdate < tomorrow);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Alias for RecordTransaction - for backward compatibility
        /// </summary>
        public bool CreateTransaction(string sbAccountId, string transactionType, decimal amount)
        {
            return RecordTransaction(sbAccountId, transactionType, amount);
        }
    }
}
