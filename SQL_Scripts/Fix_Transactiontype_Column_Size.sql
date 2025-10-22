-- ========================================
-- FIX: Resize Transactiontype Column
-- ========================================
-- Problem: Column is VARCHAR(10), too small for transaction types
-- Solution: Increase to VARCHAR(50) to accommodate all types
-- ========================================

USE Banking_Details;
GO

-- Backup existing data (optional but recommended)
SELECT * 
INTO SavingsTransaction_Backup_BeforeResize
FROM SavingsTransaction;
GO

-- Show current column size
PRINT '=== BEFORE RESIZE ==='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsTransaction' 
AND COLUMN_NAME = 'Transactiontype';
GO

-- Resize the column
ALTER TABLE SavingsTransaction
ALTER COLUMN Transactiontype VARCHAR(50) NOT NULL;
GO

-- Verify new size
PRINT '=== AFTER RESIZE ==='
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsTransaction' 
AND COLUMN_NAME = 'Transactiontype';
GO

-- Verify existing data is intact
SELECT 
    COUNT(*) as TotalTransactions,
    COUNT(DISTINCT Transactiontype) as UniqueTypes
FROM SavingsTransaction;
GO

-- Show all transaction types to verify
SELECT DISTINCT Transactiontype, COUNT(*) as Count
FROM SavingsTransaction
GROUP BY Transactiontype
ORDER BY Transactiontype;
GO

PRINT '? Transactiontype column resized to VARCHAR(50)!';
PRINT '? All existing transactions preserved!';
PRINT '? TRANSFER_CREDIT and TRANSFER_DEBIT will now work!';
GO
