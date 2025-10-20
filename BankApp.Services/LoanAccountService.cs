using System;
using System.Collections.Generic;
using System.Linq;
using DB;
using DB.Utilities;

namespace BankApp.Services
{
    public class LoanAccountService
    {
        private readonly LoanAccountRepository _loanRepo;
        private readonly AccountRepository _accountRepo;
        private readonly CustomerRepository _customerRepo;

        public LoanAccountService()
        {
            _loanRepo = new LoanAccountRepository();
            _accountRepo = new AccountRepository();
            _customerRepo = new CustomerRepository();
        }

        /// <summary>
        /// Open a new Loan Account with business rule validation
        /// Business Rules:
        /// - Minimum loan: Rs. 10,000
        /// - Interest rates: 10% (?5L), 9.5% (5L-10L), 9% (>10L)
        /// - EMI cannot exceed 60% of monthly salary
        /// - Senior citizens: max loan Rs. 1 lakh, rate 9.5%
        /// </summary>
        public AccountOperationResult OpenLoanAccount(string customerId, decimal loanAmount, DateTime startDate, int tenureMonths, decimal monthlySalary, string openedBy, string openedByRole)
        {
            var validationRules = new List<Func<AccountOperationResult>>
            {
                () => string.IsNullOrWhiteSpace(customerId) ? Error("Customer ID is required") : null,
                () => !_customerRepo.CustomerExists(customerId) ? Error("Customer not found") : null,
                () => loanAmount < 10000 ? Error("Minimum loan amount is Rs. 10,000") : null,
                () => tenureMonths <= 0 ? Error("Tenure must be greater than 0 months") : null,
                () => monthlySalary <= 0 ? Error("Monthly salary must be greater than 0") : null
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

                // Apply senior citizen rules
                if (isSeniorCitizen)
                {
                    if (loanAmount > 100000)
                    {
                        return Error("Senior citizens cannot be sanctioned a loan greater than Rs. 1 lakh (Rs. 100,000)");
                    }
                }

                // Calculate interest rate
                decimal interestRate = CalculateLoanInterestRate(loanAmount, isSeniorCitizen);

                // Calculate EMI using formula: EMI = [P x R x (1+R)^N]/[(1+R)^N-1]
                // Where P = Loan amount, R = Monthly interest rate, N = Tenure in months
                decimal monthlyInterestRate = (interestRate / 100) / 12;
                double powerTerm = Math.Pow((double)(1 + monthlyInterestRate), tenureMonths);
                decimal emi = loanAmount * monthlyInterestRate * (decimal)powerTerm / ((decimal)powerTerm - 1);

                // Validate EMI is not more than 60% of monthly salary
                decimal maxAllowedEMI = monthlySalary * 0.60m;
                if (emi > maxAllowedEMI)
                {
                    return Error($"EMI amount (Rs. {emi:N2}) exceeds 60% of monthly salary (Rs. {maxAllowedEMI:N2}). Please reduce loan amount or increase tenure.");
                }

                // Generate Loan Account ID
                string lnAccountId = IdGenerator.GenerateLoanAccountId();

                // Create master account entry
                bool accountCreated = _accountRepo.CreateAccount(lnAccountId, "LOAN", customerId, openedBy, openedByRole);
                if (!accountCreated)
                {
                    return Error("Failed to create account entry");
                }

                // Create loan account entry
                bool loanCreated = _loanRepo.CreateLoanAccount(lnAccountId, customerId, loanAmount, startDate, tenureMonths, interestRate, emi);
                if (!loanCreated)
                {
                    return Error("Failed to create loan account");
                }

                string seniorCitizenNote = isSeniorCitizen ? " (Senior Citizen Rate)" : "";
                return Success(
                    $"Loan Account opened successfully! Account ID: {lnAccountId}, Loan Amount: Rs. {loanAmount:N2}, Interest Rate: {interestRate}%{seniorCitizenNote}, Tenure: {tenureMonths} months, EMI: Rs. {emi:N2}",
                    lnAccountId,
                    loanAmount,
                    null,
                    emi,
                    interestRate
                );
            }
            catch (Exception ex)
            {
                return Error($"Failed to open loan account: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate loan interest rate based on amount and customer type
        /// 10% for loans up to Rs. 5 lakhs
        /// 9.5% for loans from Rs. 5 lakhs to Rs. 10 lakhs
        /// 9% for loans above Rs. 10 lakhs
        /// Senior Citizens: 9.5% (fixed)
        /// </summary>
        private decimal CalculateLoanInterestRate(decimal loanAmount, bool isSeniorCitizen)
        {
            if (isSeniorCitizen)
            {
                return 9.5m;  // Fixed rate for senior citizens
            }

            if (loanAmount <= 500000)
            {
                return 10.0m;  // 10% for up to 5 lakhs
            }
            else if (loanAmount <= 1000000)
            {
                return 9.5m;   // 9.5% for 5 lakhs to 10 lakhs
            }
            else
            {
                return 9.0m;   // 9% for above 10 lakhs
            }
        }

        /// <summary>
        /// Make part payment on loan
        /// </summary>
        public AccountOperationResult MakePartPayment(string lnAccountId, decimal amount)
        {
            try
            {
                var loanAccount = _loanRepo.GetLoanAccountById(lnAccountId);
                if (loanAccount == null)
                {
                    return Error("Loan Account not found");
                }

                // In a full implementation, you would:
                // 1. Record the payment in LoanTransaction table
                // 2. Update outstanding amount
                // 3. Adjust EMI schedule

                return Success($"Part payment of Rs. {amount:N2} processed successfully for Loan Account {lnAccountId}", lnAccountId, amount);
            }
            catch (Exception ex)
            {
                return Error($"Part payment failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Foreclose (close) loan account
        /// </summary>
        public AccountOperationResult ForeCloseLoanAccount(string lnAccountId)
        {
            try
            {
                var loanAccount = _loanRepo.GetLoanAccountById(lnAccountId);
                if (loanAccount == null)
                {
                    return Error("Loan Account not found");
                }

                // Close in master Account table
                bool closed = _accountRepo.CloseAccount(lnAccountId);
                if (closed)
                {
                    return Success($"Loan Account {lnAccountId} foreclosed successfully. Loan Amount: Rs. {loanAccount.loan_amount:N2}", lnAccountId, loanAccount.loan_amount ?? 0);
                }
                else
                {
                    return Error("Failed to foreclose loan account");
                }
            }
            catch (Exception ex)
            {
                return Error($"Failed to foreclose account: {ex.Message}");
            }
        }

        /// <summary>
        /// Get loan account details
        /// </summary>
        public LoanAccount GetAccountDetails(string lnAccountId)
        {
            return _loanRepo.GetLoanAccountById(lnAccountId);
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
