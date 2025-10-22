-- ============================================================
-- DIAGNOSE LOAN ACCOUNT CREATION ERROR
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'LOAN ACCOUNT TABLE DIAGNOSTICS';
PRINT '========================================';
PRINT '';

-- 1. Check LoanAccount table structure
PRINT '1. LoanAccount Table Structure:';
PRINT '------------------------------';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount'
ORDER BY ORDINAL_POSITION;
PRINT '';

-- 2. Check constraints
PRINT '2. NOT NULL Columns:';
PRINT '-------------------';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND IS_NULLABLE = 'NO';
PRINT '';

-- 3. Check foreign keys
PRINT '3. Foreign Key Constraints:';
PRINT '--------------------------';
SELECT 
    fk.name AS 'FK Name',
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS 'Column',
    OBJECT_NAME(fk.referenced_object_id) AS 'Referenced Table',
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS 'Referenced Column'
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('LoanAccount');
PRINT '';

-- 4. Check primary key
PRINT '4. Primary Key:';
PRINT '--------------';
SELECT 
    COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'LoanAccount' 
  AND CONSTRAINT_NAME LIKE 'PK%';
PRINT '';

-- 5. Check if Account exists
PRINT '5. Check if Account LN00001 exists:';
PRINT '---------------------------------';
SELECT * FROM Account WHERE AccountID = 'LN00001';

IF @@ROWCOUNT = 0
    PRINT '? Account LN00001 does NOT exist in Account table!';
ELSE
    PRINT '? Account LN00001 exists';
PRINT '';

-- 6. Check if Customer exists
PRINT '6. Check if Customer MLA00003 exists:';
PRINT '-----------------------------------';
SELECT * FROM Customer WHERE Custid = 'MLA00003';

IF @@ROWCOUNT = 0
    PRINT '? Customer MLA00003 does NOT exist!';
ELSE
    PRINT '? Customer MLA00003 exists';
PRINT '';

-- 7. Test insert
PRINT '7. Test LoanAccount Insert:';
PRINT '--------------------------';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- First ensure Account exists
    IF NOT EXISTS (SELECT 1 FROM Account WHERE AccountID = 'LN99999')
    BEGIN
        INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
        VALUES ('LN99999', 'LOAN', 'MLA00003', 'MGR001', 'MANAGER', GETDATE(), 'OPEN');
        PRINT '? Test Account created';
    END
    
    -- Try to insert loan account
    INSERT INTO LoanAccount (Ln_accountid, Customer, loan_amount, Start_date, Tenure, Ln_roi, Emi)
    VALUES ('LN99999', 'MLA00003', 50000, GETDATE(), 12, 10.0, 4387.75);
    
    PRINT '? SUCCESS! Test LoanAccount insert worked';
    
    -- Show inserted data
    SELECT * FROM LoanAccount WHERE Ln_accountid = 'LN99999';
    
    -- Clean up
    DELETE FROM LoanAccount WHERE Ln_accountid = 'LN99999';
    DELETE FROM Account WHERE AccountID = 'LN99999';
    
    ROLLBACK TRANSACTION;
    PRINT 'Test data cleaned up';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '? Test insert FAILED';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT '';
    PRINT 'This is why loan account creation is failing!';
END CATCH
PRINT '';

-- 8. Check existing loan accounts
PRINT '8. Existing Loan Accounts:';
PRINT '-------------------------';
SELECT 
    Ln_accountid,
    Customer,
    loan_amount,
    Start_date,
    Tenure,
    Ln_roi,
    Emi
FROM LoanAccount;

IF @@ROWCOUNT = 0
    PRINT 'No loan accounts found (table is empty)';
PRINT '';

-- 9. Check data types match
PRINT '9. Data Type Compatibility Check:';
PRINT '--------------------------------';
PRINT 'Checking if Entity Framework types match database types...';

DECLARE @expectedTypes TABLE (
    ColumnName VARCHAR(50),
    ExpectedType VARCHAR(50),
    ActualType VARCHAR(50),
    Match BIT
);

INSERT INTO @expectedTypes
SELECT 
    'Ln_accountid' AS ColumnName,
    'string (varchar)' AS ExpectedType,
    DATA_TYPE AS ActualType,
    CASE WHEN DATA_TYPE IN ('varchar', 'nvarchar', 'char') THEN 1 ELSE 0 END AS Match
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'Ln_accountid'

UNION ALL

SELECT 
    'Customer',
    'string (varchar)',
    DATA_TYPE,
    CASE WHEN DATA_TYPE IN ('varchar', 'nvarchar', 'char') THEN 1 ELSE 0 END
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'Customer'

UNION ALL

SELECT 
    'loan_amount',
    'decimal',
    DATA_TYPE,
    CASE WHEN DATA_TYPE IN ('decimal', 'numeric', 'money') THEN 1 ELSE 0 END
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'loan_amount'

UNION ALL

SELECT 
    'Start_date',
    'DateTime',
    DATA_TYPE,
    CASE WHEN DATA_TYPE IN ('datetime', 'date', 'datetime2') THEN 1 ELSE 0 END
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'Start_date'

UNION ALL

SELECT 
    'Tenure',
    'int',
    DATA_TYPE,
    CASE WHEN DATA_TYPE IN ('int', 'bigint', 'smallint') THEN 1 ELSE 0 END
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'Tenure'

UNION ALL

SELECT 
    'Ln_roi',
    'decimal',
    DATA_TYPE,
    CASE WHEN DATA_TYPE IN ('decimal', 'numeric', 'money', 'float') THEN 1 ELSE 0 END
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'Ln_roi'

UNION ALL

SELECT 
    'Emi',
    'decimal',
    DATA_TYPE,
    CASE WHEN DATA_TYPE IN ('decimal', 'numeric', 'money') THEN 1 ELSE 0 END
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LoanAccount' AND COLUMN_NAME = 'Emi';

SELECT 
    ColumnName,
    ExpectedType,
    ActualType,
    CASE WHEN Match = 1 THEN '? Match' ELSE '? Type Mismatch!' END AS Status
FROM @expectedTypes;
PRINT '';

PRINT '========================================';
PRINT 'DIAGNOSTIC COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'COMMON ISSUES:';
PRINT '1. FK_LoanAccount_Account constraint ? Ln_accountid must exist in Account table first';
PRINT '2. FK_LoanAccount_Customer constraint ? Customer must exist in Customer table';
PRINT '3. Start_date NOT NULL ? Cannot be empty or default(DateTime)';
PRINT '4. Tenure NOT NULL ? Must have valid value';
PRINT '5. Ln_roi NOT NULL ? Must have valid value';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Fix any ? errors shown above';
PRINT '2. Ensure Account table insert completes BEFORE LoanAccount insert';
PRINT '3. Check Output window in Visual Studio for detailed error';
PRINT '';

GO
