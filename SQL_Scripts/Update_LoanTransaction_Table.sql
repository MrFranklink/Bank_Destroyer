-- ============================================================
-- UPDATE LOANTRANSACTION TABLE FOR EMI PAYMENTS
-- ============================================================
-- Ensures proper tracking of EMI payments and outstanding balance
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'UPDATING LOANTRANSACTION TABLE';
PRINT '========================================';
PRINT '';

-- Check current structure
PRINT 'Current LoanTransaction structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanTransaction'
ORDER BY ORDINAL_POSITION;
PRINT '';

-- Add PaymentType column if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'LoanTransaction' AND COLUMN_NAME = 'PaymentType')
BEGIN
    ALTER TABLE LoanTransaction ADD PaymentType VARCHAR(20) DEFAULT 'EMI';
    PRINT '? PaymentType column added (EMI, PART_PAYMENT, FULL_CLOSURE)';
END
ELSE
BEGIN
    PRINT '??  PaymentType column already exists';
END
PRINT '';

-- Add PaidBy column if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'LoanTransaction' AND COLUMN_NAME = 'PaidBy')
BEGIN
    ALTER TABLE LoanTransaction ADD PaidBy CHAR(8);
    PRINT '? PaidBy column added (Customer ID who made payment)';
END
ELSE
BEGIN
    PRINT '??  PaidBy column already exists';
END
PRINT '';

-- Verify updated structure
PRINT 'Updated LoanTransaction structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanTransaction'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '========================================';
PRINT 'LOANTRANSACTION TABLE UPDATED!';
PRINT '========================================';
PRINT '';
PRINT 'Columns:';
PRINT '? Transactionno    INT IDENTITY    - PRIMARY KEY';
PRINT '? Ln_accountid     CHAR(7)         - Loan account';
PRINT '? Emidate          DATETIME        - Payment date';
PRINT '? Amount           DECIMAL         - Payment amount';
PRINT '? Outstanding      DECIMAL         - Remaining balance';
PRINT '? PaymentType      VARCHAR(20)     - EMI/PART_PAYMENT/FULL_CLOSURE';
PRINT '? PaidBy           CHAR(8)         - Customer ID';
PRINT '';
PRINT 'Payment Types:';
PRINT '- EMI: Regular monthly EMI payment';
PRINT '- PART_PAYMENT: Partial loan payment';
PRINT '- FULL_CLOSURE: Complete loan settlement';
PRINT '';

GO
