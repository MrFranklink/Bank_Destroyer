using System;
using System.Collections.Generic;
using System.Linq;
using DB;
using DB.Utilities;

namespace BankApp.Services
{
    public class SavingsAccountService
    {
        private readonly SavingsAccountRepository _savingsRepo;
        private readonly AccountRepository _accountRepo;
        private readonly CustomerRepository _customerRepo;

        public SavingsAccountService()
        {
            _savingsRepo = new SavingsAccountRepository();
            _accountRepo = new AccountRepository();
            _customerRepo = new CustomerRepository();
        }

        /// <summary>
        /// Open a new Savings Account with business rule validation
        /// Business Rules:
        /// - Minimum deposit: Rs. 1,000
        /// - Customer can have only 1 savings account
        /// - Employee/Manager cannot be customers
        /// </summary>
        public AccountOperationResult OpenSavingsAccount(string customerId, decimal initialDeposit, string openedBy, string openedByRole)
        {
            var validationRules = new List<Func<AccountOperationResult>>
            {
                () => string.IsNullOrWhiteSpace(customerId) ? Error("Customer ID is required") : null,
                () => !_customerRepo.CustomerExists(customerId) ? Error("Customer not found") : null,
                () => initialDeposit < 1000 ? Error("Minimum deposit for Savings Account is Rs. 1,000") : null,
                () => _savingsRepo.CustomerHasSavingsAccount(customerId) ? Error("Customer already has a Savings Account. Only one savings account allowed per customer.") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            try
            {
                // Generate Savings Account ID
                string sbAccountId = IdGenerator.GenerateSavingsAccountId();

                // Create master account entry
                bool accountCreated = _accountRepo.CreateAccount(sbAccountId, "SAVING", customerId, openedBy, openedByRole);
                if (!accountCreated)
                {
                    return Error("Failed to create account entry");
                }

                // Create savings account entry
                string errorMessage;
                bool savingsCreated = _savingsRepo.CreateSavingsAccount(sbAccountId, customerId, initialDeposit, out errorMessage);
                if (!savingsCreated)
                {
                    return Error($"Failed to create savings account: {errorMessage ?? "Unknown error"}");
                }

                return Success($"Savings Account opened successfully! Account ID: {sbAccountId}, Initial Balance: Rs. {initialDeposit:N2}", sbAccountId, initialDeposit);
            }
            catch (Exception ex)
            {
                return Error($"Failed to open savings account: {ex.Message}");
            }
        }

        /// <summary>
        /// Deposit money into savings account
        /// Business Rule: Minimum deposit Rs. 100
        /// </summary>
        public AccountOperationResult Deposit(string sbAccountId, decimal amount)
        {
            if (amount < 100)
            {
                return Error("Minimum deposit amount is Rs. 100");
            }

            try
            {
                var account = _savingsRepo.GetSavingsAccountById(sbAccountId);
                if (account == null)
                {
                    return Error("Savings Account not found");
                }

                decimal newBalance = (account.Balance ?? 0) + amount;
                bool updated = _savingsRepo.UpdateBalance(sbAccountId, newBalance);

                if (updated)
                {
                    return Success($"Deposit successful! Amount: Rs. {amount:N2}, New Balance: Rs. {newBalance:N2}", sbAccountId, newBalance);
                }
                else
                {
                    return Error("Failed to update balance");
                }
            }
            catch (Exception ex)
            {
                return Error($"Deposit failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Withdraw money from savings account
        /// Business Rules: 
        /// - Minimum withdrawal Rs. 100
        /// - Balance cannot fall below Rs. 1,000
        /// </summary>
        public AccountOperationResult Withdraw(string sbAccountId, decimal amount)
        {
            if (amount < 100)
            {
                return Error("Minimum withdrawal amount is Rs. 100");
            }

            try
            {
                var account = _savingsRepo.GetSavingsAccountById(sbAccountId);
                if (account == null)
                {
                    return Error("Savings Account not found");
                }

                decimal currentBalance = account.Balance ?? 0;
                decimal newBalance = currentBalance - amount;

                if (newBalance < 1000)
                {
                    return Error($"Insufficient balance. Minimum balance of Rs. 1,000 must be maintained. Current Balance: Rs. {currentBalance:N2}");
                }

                bool updated = _savingsRepo.UpdateBalance(sbAccountId, newBalance);

                if (updated)
                {
                    return Success($"Withdrawal successful! Amount: Rs. {amount:N2}, New Balance: Rs. {newBalance:N2}", sbAccountId, newBalance);
                }
                else
                {
                    return Error("Failed to update balance");
                }
            }
            catch (Exception ex)
            {
                return Error($"Withdrawal failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Close savings account
        /// </summary>
        public AccountOperationResult CloseSavingsAccount(string sbAccountId)
        {
            try
            {
                var account = _savingsRepo.GetSavingsAccountById(sbAccountId);
                if (account == null)
                {
                    return Error("Savings Account not found");
                }

                // Close in master Account table
                bool closed = _accountRepo.CloseAccount(sbAccountId);
                if (closed)
                {
                    return Success($"Savings Account {sbAccountId} closed successfully. Final Balance: Rs. {account.Balance:N2}", sbAccountId, account.Balance ?? 0);
                }
                else
                {
                    return Error("Failed to close account");
                }
            }
            catch (Exception ex)
            {
                return Error($"Failed to close account: {ex.Message}");
            }
        }

        /// <summary>
        /// Get savings account details
        /// </summary>
        public SavingsAccount GetAccountDetails(string sbAccountId)
        {
            return _savingsRepo.GetSavingsAccountById(sbAccountId);
        }

        private AccountOperationResult Error(string message) => new AccountOperationResult { IsSuccess = false, Message = message };
        
        private AccountOperationResult Success(string message, string accountId = null, decimal? balance = null) => 
            new AccountOperationResult 
            { 
                IsSuccess = true, 
                Message = message,
                AccountId = accountId,
                Balance = balance
            };
    }

    public class AccountOperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string AccountId { get; set; }
        public decimal? Balance { get; set; }
        public decimal? MaturityAmount { get; set; }
        public decimal? EMI { get; set; }
        public decimal? InterestRate { get; set; }
    }
}
