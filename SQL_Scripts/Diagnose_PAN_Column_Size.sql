-- ============================================================
-- CHECK PAN COLUMN SIZE IN DATABASE
-- ============================================================
-- This script checks if Pan column is VARCHAR(10) or CHAR(8)
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'CHECKING PAN COLUMN SIZE';
PRINT '========================================';
PRINT '';

-- Check Customer table
PRINT 'Customer Table - Pan Column:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'Pan';
PRINT '';

-- Check Employee table
PRINT 'Employee Table - Pan Column:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee' AND COLUMN_NAME = 'Pan';
PRINT '';

-- Check existing PAN data length
PRINT 'Existing PAN Numbers in Customer Table:';
SELECT 
    Custid,
    Pan,
    LEN(Pan) as PAN_Length,
    CASE 
        WHEN LEN(Pan) = 10 THEN 'NEW FORMAT (10 chars)'
        WHEN LEN(Pan) = 8 THEN 'OLD FORMAT (8 chars)'
        ELSE 'INVALID LENGTH'
    END as Format_Status
FROM Customer
ORDER BY LEN(Pan) DESC;
PRINT '';

PRINT 'Existing PAN Numbers in Employee Table:';
SELECT 
    Empid,
    Pan,
    LEN(Pan) as PAN_Length,
    CASE 
        WHEN LEN(Pan) = 10 THEN 'NEW FORMAT (10 chars)'
        WHEN LEN(Pan) = 8 THEN 'OLD FORMAT (8 chars)'
        ELSE 'INVALID LENGTH'
    END as Format_Status
FROM Employee
ORDER BY LEN(Pan) DESC;
PRINT '';

PRINT '========================================';
PRINT 'DIAGNOSIS:';
PRINT '========================================';
PRINT '';
PRINT 'IF CHARACTER_MAXIMUM_LENGTH = 8:';
PRINT '  ? PROBLEM: Column is CHAR(8) or VARCHAR(8)';
PRINT '  ? SOLUTION: Run Update_PAN_Format_To_Real.sql';
PRINT '';
PRINT 'IF CHARACTER_MAXIMUM_LENGTH = 10:';
PRINT '  ? CORRECT: Column size is already VARCHAR(10)';
PRINT '  ??  If still having issues, check browser cache';
PRINT '';
PRINT 'IF OLD FORMAT PANs EXIST:';
PRINT '  ??  WARNING: Old 8-character PANs in database';
PRINT '  ?? ACTION: Delete test data or migrate to 10-char format';
PRINT '';

GO
