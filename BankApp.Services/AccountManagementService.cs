using System;
using System.Collections.Generic;
using DB;

namespace BankApp.Services
{
    /// <summary>
    /// Service for account management operations
    /// </summary>
    public class AccountManagementService
    {
        private readonly AccountRepository _accountRepo;
        private readonly CustomerRepository _customerRepo;
        private readonly SavingsAccountRepository _savingsRepo;
        private readonly FixedDepositAccountRepository _fdRepo;
        private readonly LoanAccountRepository _loanRepo;

        public AccountManagementService()
        {
            _accountRepo = new AccountRepository();
            _customerRepo = new CustomerRepository();
            _savingsRepo = new SavingsAccountRepository();
            _fdRepo = new FixedDepositAccountRepository();
            _loanRepo = new LoanAccountRepository();
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        public List<AccountDTO> GetAllAccounts()
        {
            var accounts = _accountRepo.GetAllAccounts();
            var accountDTOs = new List<AccountDTO>();

            foreach (var account in accounts)
            {
                accountDTOs.Add(new AccountDTO
                {
                    AccountID = account.AccountID,
                    AccountType = account.AccountType,
                    CustomerID = account.CustomerID,
                    OpenedBy = account.OpenedBy,
                    OpenedByRole = account.OpenedByRole,
                    OpenDate = account.OpenDate,
                    Status = account.Status,
                    ClosedDate = account.ClosedDate
                });
            }

            return accountDTOs;
        }

        /// <summary>
        /// Get total account count
        /// </summary>
        public int GetTotalAccountCount()
        {
            return _accountRepo.GetTotalAccountCount();
        }

        /// <summary>
        /// Get accounts by customer ID
        /// </summary>
        public List<AccountDTO> GetAccountsByCustomerId(string customerId)
        {
            var accounts = _accountRepo.GetAccountsByCustomerId(customerId);
            var accountDTOs = new List<AccountDTO>();

            foreach (var account in accounts)
            {
                accountDTOs.Add(new AccountDTO
                {
                    AccountID = account.AccountID,
                    AccountType = account.AccountType,
                    CustomerID = account.CustomerID,
                    OpenedBy = account.OpenedBy,
                    OpenedByRole = account.OpenedByRole,
                    OpenDate = account.OpenDate,
                    Status = account.Status,
                    ClosedDate = account.ClosedDate
                });
            }

            return accountDTOs;
        }

        /// <summary>
        /// Get account by ID
        /// </summary>
        public AccountDTO GetAccountById(string accountId)
        {
            var account = _accountRepo.GetAccountById(accountId);
            if (account == null)
            {
                return null;
            }

            return new AccountDTO
            {
                AccountID = account.AccountID,
                AccountType = account.AccountType,
                CustomerID = account.CustomerID,
                OpenedBy = account.OpenedBy,
                OpenedByRole = account.OpenedByRole,
                OpenDate = account.OpenDate,
                Status = account.Status,
                ClosedDate = account.ClosedDate
            };
        }

        /// <summary>
        /// Get customer profile by customer ID
        /// </summary>
        public CustomerProfileDTO GetCustomerProfile(string customerId)
        {
            var customer = _customerRepo.GetCustomerById(customerId);
            if (customer == null)
            {
                return null;
            }

            return new CustomerProfileDTO
            {
                Custid = customer.Custid,
                Custname = customer.Custname,
                DOB = customer.DOB,
                Pan = customer.Pan,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber
            };
        }

        /// <summary>
        /// Get savings account balance
        /// </summary>
        public decimal GetSavingsBalance(string accountId)
        {
            var savingsAccount = _savingsRepo.GetSavingsAccountById(accountId);
            return savingsAccount?.Balance ?? 0;
        }

        /// <summary>
        /// Get account details with balance/amount based on account type
        /// </summary>
        public AccountDetailsDTO GetAccountDetails(string accountId, string accountType)
        {
            var accountDetailsDTO = new AccountDetailsDTO
            {
                AccountID = accountId,
                AccountType = accountType
            };

            if (accountType == "SAVING")
            {
                var savingsAccount = _savingsRepo.GetSavingsAccountById(accountId);
                if (savingsAccount != null)
                {
                    accountDetailsDTO.Balance = savingsAccount.Balance ?? 0;
                }
            }
            else if (accountType == "FIXED-DEPOSIT")
            {
                var fdAccount = _fdRepo.GetFDAccountById(accountId);
                if (fdAccount != null)
                {
                    accountDetailsDTO.Amount = fdAccount.Amount ?? 0;
                    accountDetailsDTO.MaturityAmount = fdAccount.MaturityAmount ?? 0;
                    accountDetailsDTO.InterestRate = fdAccount.FD_ROI;
                    accountDetailsDTO.StartDate = fdAccount.StartDate;
                    accountDetailsDTO.EndDate = fdAccount.EndDate;
                }
            }
            else if (accountType == "LOAN")
            {
                var loanAccount = _loanRepo.GetLoanAccountById(accountId);
                if (loanAccount != null)
                {
                    accountDetailsDTO.LoanAmount = loanAccount.loan_amount ?? 0;
                    accountDetailsDTO.EMI = loanAccount.Emi ?? 0;
                    accountDetailsDTO.InterestRate = loanAccount.Ln_roi;
                    accountDetailsDTO.Tenure = loanAccount.Tenure;
                    accountDetailsDTO.StartDate = loanAccount.Start_date;
                }
            }

            return accountDetailsDTO;
        }
    }

    /// <summary>
    /// DTO for Account to avoid exposing DB entities to UI layer
    /// </summary>
    public class AccountDTO
    {
        public string AccountID { get; set; }
        public string AccountType { get; set; }
        public string CustomerID { get; set; }
        public string OpenedBy { get; set; }
        public string OpenedByRole { get; set; }
        public DateTime OpenDate { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }
    }

    /// <summary>
    /// DTO for Customer Profile
    /// </summary>
    public class CustomerProfileDTO
    {
        public string Custid { get; set; }
        public string Custname { get; set; }
        public DateTime? DOB { get; set; }
        public string Pan { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// DTO for Account Details with type-specific information
    /// </summary>
    public class AccountDetailsDTO
    {
        public string AccountID { get; set; }
        public string AccountType { get; set; }
        
        // Savings Account
        public decimal Balance { get; set; }
        
        // Fixed Deposit
        public decimal Amount { get; set; }
        public decimal MaturityAmount { get; set; }
        
        // Loan
        public decimal LoanAmount { get; set; }
        public decimal EMI { get; set; }
        
        // Common
        public decimal InterestRate { get; set; }
        public int Tenure { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
