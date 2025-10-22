-- ========================================
-- Verify All Transaction Types Will Fit
-- ========================================

USE Banking_Details;
GO

-- List all transaction types used in the system
-- with their character lengths

PRINT '=== TRANSACTION TYPES IN SYSTEM ==='
PRINT ''

DECLARE @Types TABLE (TypeName VARCHAR(50), Length INT)

INSERT INTO @Types VALUES ('INITIAL DEPOSIT', LEN('INITIAL DEPOSIT'))
INSERT INTO @Types VALUES ('DEPOSIT', LEN('DEPOSIT'))
INSERT INTO @Types VALUES ('WITHDRAWAL', LEN('WITHDRAWAL'))
INSERT INTO @Types VALUES ('TRANSFER_DEBIT', LEN('TRANSFER_DEBIT'))
INSERT INTO @Types VALUES ('TRANSFER_CREDIT', LEN('TRANSFER_CREDIT'))
INSERT INTO @Types VALUES ('LOAN_PAYMENT', LEN('LOAN_PAYMENT'))

SELECT 
    TypeName as 'Transaction Type',
    Length as 'Characters',
    CASE 
        WHEN Length <= 10 THEN '? TOO BIG FOR VARCHAR(10)!'
        WHEN Length <= 20 THEN '??  Close to limit'
        ELSE '? Fits comfortably'
    END as 'Status (if VARCHAR(10))',
    CASE 
        WHEN Length <= 50 THEN '? Fits in VARCHAR(50)'
        ELSE '? Still too big!'
    END as 'Status (if VARCHAR(50))'
FROM @Types
ORDER BY Length DESC;

PRINT ''
PRINT '=== RECOMMENDATION ==='
PRINT 'Minimum column size: VARCHAR(20)'
PRINT 'Recommended size: VARCHAR(50) ?'
PRINT 'Current size (likely): VARCHAR(10) ?'

-- Show actual data in database
PRINT ''
PRINT '=== ACTUAL DATA IN DATABASE ==='
SELECT DISTINCT 
    Transactiontype,
    LEN(Transactiontype) as ActualLength,
    CASE 
        WHEN LEN(Transactiontype) > 10 THEN '? TRUNCATED!'
        ELSE '? OK'
    END as Status
FROM SavingsTransaction
ORDER BY ActualLength DESC;

GO
