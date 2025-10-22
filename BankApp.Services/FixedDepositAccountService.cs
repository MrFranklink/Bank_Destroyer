using System;
using System.Collections.Generic;
using System.Linq;
using DB;
using DB.Utilities;

namespace BankApp.Services
{
    public class FixedDepositAccountService
    {
        private readonly FixedDepositAccountRepository _fdRepo;
        private readonly AccountRepository _accountRepo;
        private readonly CustomerRepository _customerRepo;

        public FixedDepositAccountService()
        {
            _fdRepo = new FixedDepositAccountRepository();
            _accountRepo = new AccountRepository();
            _customerRepo = new CustomerRepository();
        }

        /// <summary>
        /// Open a new Fixed Deposit Account with business rule validation
        /// Business Rules:
        /// - Minimum deposit: Rs. 10,000
        /// - Interest rates: 6% (?1 year), 7% (1-2 years), 8% (>2 years)
        /// - Senior citizens get +0.5% extra interest
        /// </summary>
        public AccountOperationResult OpenFixedDepositAccount(string customerId, decimal amount, DateTime startDate, int tenureMonths, string openedBy, string openedByRole)
        {
            var validationRules = new List<Func<AccountOperationResult>>
            {
                () => string.IsNullOrWhiteSpace(customerId) ? Error("Customer ID is required") : null,
                () => !_customerRepo.CustomerExists(customerId) ? Error($"Customer ID '{customerId}' not found in the system") : null,
                () => amount < 10000 ? Error("Minimum deposit for Fixed Deposit is Rs. 10,000") : null,
                () => startDate.Date < DateTime.Now.Date ? Error("Start date cannot be in the past. Please select today or a future date.") : null,
                () => tenureMonths <= 0 ? Error("Tenure must be greater than 0 months") : null,
                () => tenureMonths > 360 ? Error("Maximum tenure is 360 months (30 years)") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            try
            {
                // Get customer to check if senior citizen
                var customer = _customerRepo.GetCustomerById(customerId);
                if (customer == null)
                {
                    return Error("Customer not found");
                }

                bool isSeniorCitizen = customer.DOB.HasValue && IdGenerator.IsSeniorCitizen(customer.DOB.Value);

                // Calculate interest rate based on tenure
                decimal interestRate = CalculateInterestRate(tenureMonths, isSeniorCitizen);

                // Calculate end date
                DateTime endDate = startDate.AddMonths(tenureMonths);

                // Calculate maturity amount using compound interest formula
                // A = P(1 + r/n)^(nt)
                // For simplicity, using annual compounding
                double years = tenureMonths / 12.0;
                decimal maturityAmount = amount * (decimal)Math.Pow((double)(1 + interestRate / 100), years);

                // Generate FD Account ID
                string fdAccountId = IdGenerator.GenerateFixedDepositAccountId();

                // Create master account entry
                bool accountCreated = _accountRepo.CreateAccount(fdAccountId, "FIXED-DEPOSIT", customerId, openedBy, openedByRole);
                if (!accountCreated)
                {
                    return Error("Failed to create account entry");
                }

                // Create FD account entry
                bool fdCreated = _fdRepo.CreateFixedDepositAccount(fdAccountId, customerId, amount, startDate, endDate, interestRate, maturityAmount);
                if (!fdCreated)
                {
                    return Error("Failed to create fixed deposit account");
                }

                string seniorCitizenBonus = isSeniorCitizen ? " (includes +0.5% senior citizen bonus)" : "";
                return Success(
                    $"Fixed Deposit opened successfully! Account ID: {fdAccountId}, Amount: Rs. {amount:N2}, Interest Rate: {interestRate}%{seniorCitizenBonus}, Maturity Amount: Rs. {maturityAmount:N2}",
                    fdAccountId,
                    amount,
                    maturityAmount,
                    null,
                    interestRate
                );
            }
            catch (Exception ex)
            {
                return Error($"Failed to open fixed deposit: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate FD interest rate based on tenure
        /// 6% for up to 1 year
        /// 7% for 1 to 2 years
        /// 8% for more than 2 years
        /// +0.5% for senior citizens
        /// </summary>
        private decimal CalculateInterestRate(int tenureMonths, bool isSeniorCitizen)
        {
            decimal baseRate;

            if (tenureMonths <= 12)
            {
                baseRate = 6.0m;  // 6% for up to 1 year
            }
            else if (tenureMonths <= 24)
            {
                baseRate = 7.0m;  // 7% for 1-2 years
            }
            else
            {
                baseRate = 8.0m;  // 8% for more than 2 years
            }

            // Add 0.5% bonus for senior citizens
            if (isSeniorCitizen)
            {
                baseRate += 0.5m;
            }

            return baseRate;
        }

        /// <summary>
        /// Foreclose (close) FD account before maturity
        /// </summary>
        public AccountOperationResult ForeCloseFDAccount(string fdAccountId)
        {
            try
            {
                var fdAccount = _fdRepo.GetFDAccountById(fdAccountId);
                if (fdAccount == null)
                {
                    return Error("Fixed Deposit Account not found");
                }

                // Close in master Account table
                bool closed = _accountRepo.CloseAccount(fdAccountId);
                if (closed)
                {
                    // Calculate premature withdrawal amount (usually with penalty, but not specified in requirements)
                    decimal amountToReturn = fdAccount.Amount ?? 0;
                    return Success($"Fixed Deposit {fdAccountId} foreclosed. Amount returned: Rs. {amountToReturn:N2}", fdAccountId, amountToReturn);
                }
                else
                {
                    return Error("Failed to foreclose account");
                }
            }
            catch (Exception ex)
            {
                return Error($"Failed to foreclose account: {ex.Message}");
            }
        }

        /// <summary>
        /// Get FD account details
        /// </summary>
        public FixedDepositAccount GetAccountDetails(string fdAccountId)
        {
            return _fdRepo.GetFDAccountById(fdAccountId);
        }

        private AccountOperationResult Error(string message) => new AccountOperationResult { IsSuccess = false, Message = message };
        
        private AccountOperationResult Success(string message, string accountId = null, decimal? balance = null, decimal? maturityAmount = null, decimal? emi = null, decimal? interestRate = null) => 
            new AccountOperationResult 
            { 
                IsSuccess = true, 
                Message = message,
                AccountId = accountId,
                Balance = balance,
                MaturityAmount = maturityAmount,
                EMI = emi,
                InterestRate = interestRate
            };
    }
}
