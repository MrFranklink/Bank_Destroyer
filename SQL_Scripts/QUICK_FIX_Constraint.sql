-- ========================================
-- QUICK FIX: One-Command Solution
-- ========================================
-- Just copy and paste this entire block into SQL Server Management Studio
-- and press F5 to execute
-- ========================================

USE Banking_Details;
GO

-- Drop old constraint
ALTER TABLE SavingsTransaction DROP CONSTRAINT CK__SavingsTr__Trans__73852659;

-- Create new constraint with all types
ALTER TABLE SavingsTransaction
ADD CONSTRAINT CK_SavingsTransaction_Transactiontype
CHECK (Transactiontype IN ('DEPOSIT', 'WITHDRAW', 'WITHDRAWAL', 'INITIAL DEPOSIT', 'TRANSFER_DEBIT', 'TRANSFER_CREDIT', 'LOAN_PAYMENT'));

-- Verify
SELECT 'Fixed!' as Status, CONSTRAINT_NAME, CHECK_CLAUSE 
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS cc
INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu ON cc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
WHERE cu.TABLE_NAME = 'SavingsTransaction' AND cu.COLUMN_NAME = 'Transactiontype';

GO
