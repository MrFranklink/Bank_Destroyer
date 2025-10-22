using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class FundTransferRepository
    {
        /// <summary>
        /// Create a fund transfer record
        /// </summary>
        public bool CreateFundTransfer(string fromAccountId, string toAccountId, decimal amount, string fromCustomerId, string toCustomerId, string status, string remarks)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var transfer = new FundTransfer
                    {
                        FromAccountID = fromAccountId,
                        ToAccountID = toAccountId,
                        Amount = amount,
                        TransferDate = DateTime.Now,
                        FromCustomerID = fromCustomerId,
                        ToCustomerID = toCustomerId,
                        Status = status,
                        Remarks = remarks
                    };

                    context.FundTransfers.Add(transfer);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in CreateFundTransfer: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get all transfers for a customer (sent + received)
        /// </summary>
        public List<FundTransfer> GetTransfersByCustomerId(string customerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.FundTransfers
                        .Where(ft => ft.FromCustomerID == customerId || ft.ToCustomerID == customerId)
                        .OrderByDescending(ft => ft.TransferDate)
                        .ToList();
                }
            }
            catch
            {
                return new List<FundTransfer>();
            }
        }

        /// <summary>
        /// Get transfers by account (from specific account)
        /// </summary>
        public List<FundTransfer> GetTransfersByAccountId(string accountId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.FundTransfers
                        .Where(ft => ft.FromAccountID == accountId || ft.ToAccountID == accountId)
                        .OrderByDescending(ft => ft.TransferDate)
                        .ToList();
                }
            }
            catch
            {
                return new List<FundTransfer>();
            }
        }

        /// <summary>
        /// Get daily transfer total for a customer (for limit checking)
        /// </summary>
        public decimal GetDailyTransferTotal(string fromAccountId, DateTime date)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.FundTransfers
                        .Where(ft => ft.FromAccountID == fromAccountId 
                                  && ft.TransferDate >= date.Date 
                                  && ft.TransferDate < date.Date.AddDays(1)
                                  && ft.Status == "SUCCESS")
                        .Sum(ft => (decimal?)ft.Amount) ?? 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
