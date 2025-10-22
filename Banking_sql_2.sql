USE Banking_Details;
GO

ALTER TABLE SavingsTransaction
ALTER COLUMN Transactiontype VARCHAR(50) NOT NULL;
GO

-- Check what constraint exists
SELECT 
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE CONSTRAINT_NAME LIKE '%SavingsTr%Trans%';

USE Banking_Details;
GO

-- Drop old constraint
ALTER TABLE SavingsTransaction DROP CONSTRAINT CK__SavingsTr__Trans__73852659;

-- Create new constraint with all types
ALTER TABLE SavingsTransaction
ADD CONSTRAINT CK_SavingsTransaction_Transactiontype
CHECK (Transactiontype IN ('DEPOSIT', 'WITHDRAW', 'WITHDRAWAL', 'INITIAL DEPOSIT', 'TRANSFER_DEBIT', 'TRANSFER_CREDIT', 'LOAN_PAYMENT'));

PRINT '✅ Fixed! All transaction types now allowed!';

Delete  from Customer where Custid='MLA00007'

Delete from Account where AccountID='SB00006'

Select * from SavingsAccount

Delete from SavingsAccount where SBAccountID='SB00006'

Select * from SavingsTransaction

Delete from SavingsTransaction where Transactionid=23

Select * from FundTransfer

Delete from FundTransfer where FromAccountID='SB00006'

Select * from Account
GO