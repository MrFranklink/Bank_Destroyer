-- ========================================
-- Find Check Constraint on Transactiontype
-- ========================================

USE Banking_Details;
GO

-- Method 1: Find constraint name and definition
PRINT '=== CHECK CONSTRAINTS ON SavingsTransaction TABLE ==='
PRINT ''

SELECT 
    cc.CONSTRAINT_NAME as 'Constraint Name',
    cc.CHECK_CLAUSE as 'Check Clause (Rules)'
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS cc
INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu 
    ON cc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
WHERE cu.TABLE_NAME = 'SavingsTransaction'
AND cu.COLUMN_NAME = 'Transactiontype';

-- Method 2: Get detailed information
PRINT ''
PRINT '=== DETAILED CONSTRAINT INFO ==='
PRINT ''

SELECT 
    con.name AS 'Constraint Name',
    col.name AS 'Column Name',
    con.definition AS 'Full Definition'
FROM sys.check_constraints con
INNER JOIN sys.columns col 
    ON con.parent_object_id = col.object_id 
    AND con.parent_column_id = col.column_id
WHERE OBJECT_NAME(con.parent_object_id) = 'SavingsTransaction'
AND col.name = 'Transactiontype';

-- Method 3: Show all constraints on the table
PRINT ''
PRINT '=== ALL CONSTRAINTS ON SavingsTransaction ==='
PRINT ''

SELECT 
    tc.CONSTRAINT_TYPE as 'Type',
    tc.CONSTRAINT_NAME as 'Name'
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
WHERE tc.TABLE_NAME = 'SavingsTransaction'
ORDER BY tc.CONSTRAINT_TYPE;

GO

PRINT ''
PRINT '=== INSTRUCTIONS ==='
PRINT 'Copy the CHECK_CLAUSE or definition and send it to me!'
PRINT 'It will look something like:'
PRINT '([Transactiontype]=''DEPOSIT'' OR [Transactiontype]=''WITHDRAWAL'')'
PRINT ''
GO
