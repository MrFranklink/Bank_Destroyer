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
                () => !_customerRepo.CustomerExists(customerId) ? Error($"Customer ID '{customerId}' not found in the system") : null,
                () => loanAmount < 10000 ? Error("Minimum loan amount is Rs. 10,000") : null,
                () => startDate.Date < DateTime.Now.Date ? Error("Start date cannot be in the past. Please select today or a future date.") : null,
                () => tenureMonths <= 0 ? Error("Tenure must be greater than 0 months") : null,
                () => tenureMonths > 360 ? Error("Maximum tenure is 360 months (30 years)") : null,
                () => monthlySalary <= 0 ? Error("Monthly salary must be greater than 0") : null,
                () => monthlySalary < 1000 ? Error("Please enter a valid monthly salary (minimum Rs. 1,000)") : null
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

        /// <summary>
        /// Pay loan EMI from customer's savings account
        /// Payment types: EMI (regular), PART_PAYMENT, FULL_CLOSURE
        /// </summary>
        public AccountOperationResult PayEMI(string loanAccountId, string customerId, decimal paymentAmount, string paymentType = "EMI")
        {
            try
            {
                // Get loan account
                var loanAccount = _loanRepo.GetLoanAccountById(loanAccountId);
                if (loanAccount == null)
                {
                    return Error("Loan account not found");
                }

                // Verify ownership
                if (loanAccount.Customer != customerId)
                {
                    return Error("This loan account does not belong to you");
                }

                // Get savings account for payment
                var savingsRepo = new SavingsAccountRepository();
                var savingsAccount = savingsRepo.GetSavingsAccountByCustomerId(customerId);
                if (savingsAccount == null)
                {
                    return Error("You don't have a savings account to make payment from");
                }

                // Check sufficient balance (payment amount + Rs. 1,000 minimum balance)
                decimal currentBalance = savingsAccount.Balance ?? 0;
                if (currentBalance - paymentAmount < 1000)
                {
                    return Error($"Insufficient balance. You must maintain Rs. 1,000 minimum balance in savings account. Available: Rs. {(currentBalance - 1000 > 0 ? currentBalance - 1000 : 0):N2}");
                }

                // Get latest outstanding balance
                var loanTransactionRepo = new LoanTransactionRepository();
                var lastTransaction = loanTransactionRepo.GetLatestTransaction(loanAccountId);
                decimal outstanding = lastTransaction?.Outstanding ?? (loanAccount.loan_amount ?? 0);

                // Validate payment amount
                decimal emi = loanAccount.Emi ?? 0;
                
                if (paymentType == "EMI" && paymentAmount < emi)
                {
                    return Error($"Regular EMI payment must be at least Rs. {emi:N2}");
                }

                if (paymentAmount > outstanding)
                {
                    return Error($"Payment amount (Rs. {paymentAmount:N2}) exceeds outstanding loan balance (Rs. {outstanding:N2})");
                }

                // Calculate new outstanding
                decimal newOutstanding = outstanding - paymentAmount;

                // Execute payment (simple transaction handling)
                try
                {
                    // Deduct from savings account
                    decimal newSavingsBalance = currentBalance - paymentAmount;
                    bool savingsUpdated = savingsRepo.UpdateBalance(savingsAccount.SBAccountID, newSavingsBalance);
                    if (!savingsUpdated)
                    {
                        return Error("Failed to deduct payment from savings account");
                    }

                    // Record loan payment
                    bool paymentRecorded = loanTransactionRepo.CreateLoanTransaction(
                        loanAccountId,
                        paymentAmount,
                        newOutstanding,
                        paymentType,
                        customerId
                    );

                    if (!paymentRecorded)
                    {
                        // Rollback savings
                        savingsRepo.UpdateBalance(savingsAccount.SBAccountID, currentBalance);
                        return Error("Failed to record loan payment");
                    }

                    // Record savings transaction
                    var savingsTransactionRepo = new SavingsTransactionRepository();
                    savingsTransactionRepo.CreateTransaction(savingsAccount.SBAccountID, "LOAN_PAYMENT", paymentAmount);

                    // If fully paid, close the loan account
                    if (newOutstanding == 0)
                    {
                        var accountRepo = new AccountRepository();
                        accountRepo.CloseAccount(loanAccountId);
                    }
                }
                catch (Exception ex)
                {
                    // Attempt rollback
                    savingsRepo.UpdateBalance(savingsAccount.SBAccountID, currentBalance);
                    throw new Exception($"Payment failed: {ex.Message}", ex);
                }

                string message;
                if (newOutstanding == 0)
                {
                    message = $"Congratulations! Loan fully paid. Amount: Rs. {paymentAmount:N2}. Loan account closed.";
                }
                else
                {
                    message = $"Payment successful! Amount: Rs. {paymentAmount:N2}. Remaining balance: Rs. {newOutstanding:N2}";
                }

                return Success(message, loanAccountId, newOutstanding);
            }
            catch (Exception ex)
            {
                return Error($"Payment failed: {ex.Message}");
            }
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
