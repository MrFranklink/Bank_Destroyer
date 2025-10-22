using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class LoanTransactionRepository
    {
        /// <summary>
        /// Create a loan EMI payment transaction
        /// </summary>
        public bool CreateLoanTransaction(string lnAccountId, decimal amount, decimal outstanding, string paymentType, string paidBy)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var transaction = new LoanTransaction
                    {
                        Ln_accountid = lnAccountId,
                        Emidate = DateTime.Now,
                        Amount = amount,
                        Outstanding = outstanding
                        // Note: PaymentType and PaidBy columns need to be added to database first
                        // Will be added via SQL script
                    };

                    context.LoanTransactions.Add(transaction);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in CreateLoanTransaction: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get latest transaction for a loan account (to get current outstanding)
        /// </summary>
        public LoanTransaction GetLatestTransaction(string lnAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.LoanTransactions
                        .Where(lt => lt.Ln_accountid == lnAccountId)
                        .OrderByDescending(lt => lt.Emidate)
                        .FirstOrDefault();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get all transactions for a loan account
        /// </summary>
        public List<LoanTransaction> GetTransactionsByLoanId(string lnAccountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.LoanTransactions
                        .Where(lt => lt.Ln_accountid == lnAccountId)
                        .OrderByDescending(lt => lt.Emidate)
                        .ToList();
                }
            }
            catch
            {
                return new List<LoanTransaction>();
            }
        }

        /// <summary>
        /// Get all transactions for a customer (across all loans)
        /// </summary>
        public List<LoanTransaction> GetTransactionsByCustomerId(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.LoanTransactions
                        .Where(lt => lt.LoanAccount.Customer == customerId)
                        .OrderByDescending(lt => lt.Emidate)
                        .ToList();
                }
            }
            catch
            {
                return new List<LoanTransaction>();
            }
        }
    }
}
