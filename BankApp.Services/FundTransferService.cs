using System;
using System.Collections.Generic;
using System.Linq;
using DB;

namespace BankApp.Services
{
    public class FundTransferService
    {
        private readonly FundTransferRepository _transferRepo;
        private readonly SavingsAccountRepository _savingsRepo;
        private readonly CustomerRepository _customerRepo;
        private readonly SavingsTransactionRepository _transactionRepo;

        public FundTransferService()
        {
            _transferRepo = new FundTransferRepository();
            _savingsRepo = new SavingsAccountRepository();
            _customerRepo = new CustomerRepository();
            _transactionRepo = new SavingsTransactionRepository();
        }

        /// <summary>
        /// Transfer funds from one savings account to another
        /// Business Rules:
        /// - Min: Rs. 100, Max: Rs. 1,00,000 per transaction
        /// - Daily limit: Rs. 5,00,000
        /// - Cannot transfer to same account
        /// - Sufficient balance required (+ Rs. 1,000 minimum balance)
        /// </summary>
        public TransferResult TransferFunds(string fromCustomerId, string toAccountId, decimal amount, string remarks = null)
        {
            var validationRules = new List<Func<TransferResult>>
            {
                () => string.IsNullOrWhiteSpace(fromCustomerId) ? Error("Customer ID is required") : null,
                () => string.IsNullOrWhiteSpace(toAccountId) ? Error("Recipient account ID is required") : null,
                () => amount < 100 ? Error("Minimum transfer amount is Rs. 100") : null,
                () => amount > 100000 ? Error("Maximum transfer amount is Rs. 1,00,000 per transaction") : null,
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            try
            {
                // Get sender's savings account
                var fromAccount = _savingsRepo.GetSavingsAccountByCustomerId(fromCustomerId);
                if (fromAccount == null)
                {
                    return Error("You don't have a savings account");
                }

                // Get receiver's account
                var toAccount = _savingsRepo.GetSavingsAccountById(toAccountId);
                if (toAccount == null)
                {
                    return Error($"Recipient account '{toAccountId}' not found");
                }

                // Check if trying to transfer to self
                if (fromAccount.SBAccountID == toAccount.SBAccountID)
                {
                    return Error("Cannot transfer to your own account");
                }

                // Check account status
                var fromAccountMaster = new AccountRepository().GetAccountById(fromAccount.SBAccountID);
                var toAccountMaster = new AccountRepository().GetAccountById(toAccount.SBAccountID);

                if (fromAccountMaster.Status != "OPEN")
                {
                    return Error("Your savings account is not active");
                }

                if (toAccountMaster.Status != "OPEN")
                {
                    return Error("Recipient account is not active");
                }

                // Check sufficient balance (amount + Rs. 1,000 minimum balance)
                decimal currentBalance = fromAccount.Balance ?? 0;
                if (currentBalance - amount < 1000)
                {
                    return Error($"Insufficient balance. You must maintain Rs. 1,000 minimum balance. Available for transfer: Rs. {(currentBalance - 1000 > 0 ? currentBalance - 1000 : 0):N2}");
                }

                // Check daily transfer limit (Rs. 5,00,000)
                decimal todayTotal = _transferRepo.GetDailyTransferTotal(fromAccount.SBAccountID, DateTime.Now);
                if (todayTotal + amount > 500000)
                {
                    decimal remaining = 500000 - todayTotal;
                    return Error($"Daily transfer limit of Rs. 5,00,000 exceeded. You can transfer Rs. {remaining:N2} more today.");
                }

                // Calculate new balances
                decimal newFromBalance = currentBalance - amount;
                decimal newToBalance = (toAccount.Balance ?? 0) + amount;

                // Execute transfer (database transaction will handle atomicity)
                // Note: Using multiple SaveChanges calls - ideally should use DB transaction
                try
                {
                    // Update sender balance
                    bool fromUpdated = _savingsRepo.UpdateBalance(fromAccount.SBAccountID, newFromBalance);
                    if (!fromUpdated)
                    {
                        return Error("Failed to deduct amount from your account");
                    }

                    // Update receiver balance
                    bool toUpdated = _savingsRepo.UpdateBalance(toAccount.SBAccountID, newToBalance);
                    if (!toUpdated)
                    {
                        // Rollback sender balance
                        _savingsRepo.UpdateBalance(fromAccount.SBAccountID, currentBalance);
                        return Error("Failed to credit amount to recipient account");
                    }

                    // Record sender transaction (Debit)
                    _transactionRepo.CreateTransaction(fromAccount.SBAccountID, "TRANSFER_DEBIT", amount);

                    // Record receiver transaction (Credit)
                    _transactionRepo.CreateTransaction(toAccount.SBAccountID, "TRANSFER_CREDIT", amount);

                    // Record fund transfer
                    string transferRemarks = string.IsNullOrWhiteSpace(remarks) 
                        ? $"Transfer to {toAccount.Customerid}" 
                        : remarks;

                    bool transferRecorded = _transferRepo.CreateFundTransfer(
                        fromAccount.SBAccountID,
                        toAccount.SBAccountID,
                        amount,
                        fromCustomerId,
                        toAccount.Customerid,
                        "SUCCESS",
                        transferRemarks
                    );

                    if (!transferRecorded)
                    {
                        return Error("Transfer completed but record creation failed");
                    }
                }
                catch (Exception ex)
                {
                    // Attempt rollback on error
                    _savingsRepo.UpdateBalance(fromAccount.SBAccountID, currentBalance);
                    throw new Exception($"Transfer failed: {ex.Message}", ex);
                }

                // Get recipient details for display
                var recipientCustomer = _customerRepo.GetCustomerById(toAccount.Customerid);
                string recipientName = recipientCustomer?.Custname ?? "Unknown";

                return Success(
                    $"Transfer successful! Rs. {amount:N2} transferred to {recipientName} ({toAccountId}). Your new balance: Rs. {newFromBalance:N2}",
                    fromAccount.SBAccountID,
                    toAccountId,
                    amount,
                    newFromBalance,
                    newToBalance
                );
            }
            catch (Exception ex)
            {
                return Error($"Transfer failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get transfer history for a customer
        /// </summary>
        public List<FundTransferDTO> GetTransferHistory(string customerId)
        {
            var transfers = _transferRepo.GetTransfersByCustomerId(customerId);
            return transfers.Select(t => new FundTransferDTO
            {
                TransferID = t.TransferID,
                FromAccountID = t.FromAccountID,
                ToAccountID = t.ToAccountID,
                Amount = t.Amount,
                TransferDate = t.TransferDate,
                FromCustomerID = t.FromCustomerID,
                ToCustomerID = t.ToCustomerID,
                Status = t.Status,
                Remarks = t.Remarks,
                IsSent = t.FromCustomerID == customerId,
                IsReceived = t.ToCustomerID == customerId
            }).ToList();
        }

        private TransferResult Error(string message) => new TransferResult { IsSuccess = false, Message = message };
        
        private TransferResult Success(string message, string fromAccountId = null, string toAccountId = null, decimal? amount = null, decimal? newFromBalance = null, decimal? newToBalance = null) => 
            new TransferResult 
            { 
                IsSuccess = true, 
                Message = message,
                FromAccountID = fromAccountId,
                ToAccountID = toAccountId,
                Amount = amount,
                NewFromBalance = newFromBalance,
                NewToBalance = newToBalance
            };
    }

    public class TransferResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string FromAccountID { get; set; }
        public string ToAccountID { get; set; }
        public decimal? Amount { get; set; }
        public decimal? NewFromBalance { get; set; }
        public decimal? NewToBalance { get; set; }
    }

    public class FundTransferDTO
    {
        public int TransferID { get; set; }
        public string FromAccountID { get; set; }
        public string ToAccountID { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransferDate { get; set; }
        public string FromCustomerID { get; set; }
        public string ToCustomerID { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public bool IsSent { get; set; }
        public bool IsReceived { get; set; }
    }
}
