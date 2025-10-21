using System;
using System.Collections.Generic;
using System.Linq;
using DB;

namespace BankApp.Services
{
    /// <summary>
    /// Service for handling savings account transactions (deposits and withdrawals)
    /// </summary>
    public class SavingsTransactionService
    {
        private readonly SavingsTransactionRepository _transactionRepo;
        private readonly SavingsAccountRepository _savingsRepo;

        public SavingsTransactionService()
        {
            _transactionRepo = new SavingsTransactionRepository();
            _savingsRepo = new SavingsAccountRepository();
        }

        /// <summary>
        /// Deposit money into savings account
        /// Business Rules:
        /// - Minimum deposit: Rs. 100
        /// - Update balance in SavingsAccount table
        /// - Record transaction in SavingsTransaction table
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <param name="amount">Deposit amount</param>
        /// <returns>Transaction result</returns>
        public TransactionResult Deposit(string sbAccountId, decimal amount)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(sbAccountId))
            {
                return Error("Savings Account ID is required");
            }

            if (amount < 100)
            {
                return Error("Minimum deposit amount is Rs. 100");
            }

            if (amount <= 0)
            {
                return Error("Deposit amount must be greater than zero");
            }

            try
            {
                // Get account
                var account = _savingsRepo.GetSavingsAccountById(sbAccountId);
                if (account == null)
                {
                    return Error($"Savings Account {sbAccountId} not found");
                }

                // Calculate new balance
                decimal currentBalance = account.Balance ?? 0;
                decimal newBalance = currentBalance + amount;

                // Update balance
                bool balanceUpdated = _savingsRepo.UpdateBalance(sbAccountId, newBalance);
                if (!balanceUpdated)
                {
                    return Error("Failed to update account balance");
                }

                // Record transaction
                bool transactionRecorded = _transactionRepo.RecordTransaction(sbAccountId, "DEPOSIT", amount);
                if (!transactionRecorded)
                {
                    // Rollback balance (simple rollback, in production use transactions)
                    _savingsRepo.UpdateBalance(sbAccountId, currentBalance);
                    return Error("Failed to record transaction");
                }

                return Success(
                    $"Deposit successful! Amount: Rs. {amount:N2}",
                    sbAccountId,
                    "DEPOSIT",
                    amount,
                    currentBalance,
                    newBalance
                );
            }
            catch (Exception ex)
            {
                return Error($"Deposit failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Withdraw money from savings account
        /// Business Rules:
        /// - Minimum withdrawal: Rs. 100
        /// - Balance cannot fall below Rs. 1,000
        /// - Update balance in SavingsAccount table
        /// - Record transaction in SavingsTransaction table
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <param name="amount">Withdrawal amount</param>
        /// <returns>Transaction result</returns>
        public TransactionResult Withdraw(string sbAccountId, decimal amount)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(sbAccountId))
            {
                return Error("Savings Account ID is required");
            }

            if (amount < 100)
            {
                return Error("Minimum withdrawal amount is Rs. 100");
            }

            if (amount <= 0)
            {
                return Error("Withdrawal amount must be greater than zero");
            }

            try
            {
                // Get account
                var account = _savingsRepo.GetSavingsAccountById(sbAccountId);
                if (account == null)
                {
                    return Error($"Savings Account {sbAccountId} not found");
                }

                // Check balance
                decimal currentBalance = account.Balance ?? 0;
                decimal newBalance = currentBalance - amount;

                if (newBalance < 1000)
                {
                    return Error($"Insufficient balance. Minimum balance of Rs. 1,000 must be maintained. Current Balance: Rs. {currentBalance:N2}, Withdrawal: Rs. {amount:N2}, Remaining: Rs. {newBalance:N2}");
                }

                // Update balance
                bool balanceUpdated = _savingsRepo.UpdateBalance(sbAccountId, newBalance);
                if (!balanceUpdated)
                {
                    return Error("Failed to update account balance");
                }

                // Record transaction
                bool transactionRecorded = _transactionRepo.RecordTransaction(sbAccountId, "WITHDRAW", amount);
                if (!transactionRecorded)
                {
                    // Rollback balance
                    _savingsRepo.UpdateBalance(sbAccountId, currentBalance);
                    return Error("Failed to record transaction");
                }

                return Success(
                    $"Withdrawal successful! Amount: Rs. {amount:N2}",
                    sbAccountId,
                    "WITHDRAW",
                    amount,
                    currentBalance,
                    newBalance
                );
            }
            catch (Exception ex)
            {
                return Error($"Withdrawal failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get transaction history for an account
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>List of transactions</returns>
        public List<SavingsTransactionDTO> GetTransactionHistory(string sbAccountId)
        {
            var transactions = _transactionRepo.GetTransactionsByAccountId(sbAccountId);
            var transactionDTOs = new List<SavingsTransactionDTO>();

            foreach (var transaction in transactions)
            {
                transactionDTOs.Add(new SavingsTransactionDTO
                {
                    Transactionid = transaction.Transactionid,
                    SBAccountID = transaction.SBAccountID,
                    Transationdate = transaction.Transationdate,
                    Transactiontype = transaction.Transactiontype,
                    Amount = transaction.Amount
                });
            }

            return transactionDTOs;
        }

        /// <summary>
        /// Get transaction history for a date range
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of transactions</returns>
        public List<SavingsTransactionDTO> GetTransactionHistory(string sbAccountId, DateTime startDate, DateTime endDate)
        {
            var transactions = _transactionRepo.GetTransactionsByDateRange(sbAccountId, startDate, endDate);
            var transactionDTOs = new List<SavingsTransactionDTO>();

            foreach (var transaction in transactions)
            {
                transactionDTOs.Add(new SavingsTransactionDTO
                {
                    Transactionid = transaction.Transactionid,
                    SBAccountID = transaction.SBAccountID,
                    Transationdate = transaction.Transationdate,
                    Transactiontype = transaction.Transactiontype,
                    Amount = transaction.Amount
                });
            }

            return transactionDTOs;
        }

        /// <summary>
        /// Get all transactions (for manager reports)
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>List of all transactions</returns>
        public List<SavingsTransactionDTO> GetAllTransactions(DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = _transactionRepo.GetAllTransactions(startDate, endDate);
            var transactionDTOs = new List<SavingsTransactionDTO>();

            foreach (var transaction in transactions)
            {
                transactionDTOs.Add(new SavingsTransactionDTO
                {
                    Transactionid = transaction.Transactionid,
                    SBAccountID = transaction.SBAccountID,
                    Transationdate = transaction.Transationdate,
                    Transactiontype = transaction.Transactiontype,
                    Amount = transaction.Amount
                });
            }

            return transactionDTOs;
        }

        /// <summary>
        /// Get account summary with transaction stats
        /// </summary>
        /// <param name="sbAccountId">Savings Account ID</param>
        /// <returns>Account summary</returns>
        public AccountSummary GetAccountSummary(string sbAccountId)
        {
            var account = _savingsRepo.GetSavingsAccountById(sbAccountId);
            if (account == null)
            {
                return null;
            }

            return new AccountSummary
            {
                AccountId = sbAccountId,
                CustomerId = account.Customerid,
                CurrentBalance = account.Balance ?? 0,
                TotalDeposits = _transactionRepo.GetTotalDeposits(sbAccountId),
                TotalWithdrawals = _transactionRepo.GetTotalWithdrawals(sbAccountId),
                TransactionCount = _transactionRepo.GetTransactionCount(sbAccountId),
                LastTransactionDate = _transactionRepo.GetLastTransactionDate(sbAccountId)
            };
        }

        private TransactionResult Error(string message) => new TransactionResult { IsSuccess = false, Message = message };

        private TransactionResult Success(string message, string accountId, string transactionType, decimal amount, decimal oldBalance, decimal newBalance) =>
            new TransactionResult
            {
                IsSuccess = true,
                Message = message,
                AccountId = accountId,
                TransactionType = transactionType,
                Amount = amount,
                OldBalance = oldBalance,
                NewBalance = newBalance,
                TransactionDate = DateTime.Now
            };
    }

    /// <summary>
    /// DTO for SavingsTransaction to avoid exposing DB entities
    /// </summary>
    public class SavingsTransactionDTO
    {
        public int Transactionid { get; set; }
        public string SBAccountID { get; set; }
        public DateTime? Transationdate { get; set; }
        public string Transactiontype { get; set; }
        public decimal? Amount { get; set; }
    }

    /// <summary>
    /// Result object for transaction operations
    /// </summary>
    public class TransactionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string AccountId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    /// <summary>
    /// Account summary with transaction statistics
    /// </summary>
    public class AccountSummary
    {
        public string AccountId { get; set; }
        public string CustomerId { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}
