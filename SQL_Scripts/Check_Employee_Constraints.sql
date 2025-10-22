-- ============================================================
-- CHECK EMPLOYEE TABLE CONSTRAINTS
-- ============================================================
-- This shows ALL constraints on Employee table
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'EMPLOYEE TABLE CONSTRAINTS CHECK';
PRINT '========================================';
PRINT '';

-- 1. Check column definitions
PRINT '1. Column Definitions:';
PRINT '---------------------';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee'
ORDER BY ORDINAL_POSITION;
PRINT '';

-- 2. Check NOT NULL constraints
PRINT '2. NOT NULL Constraints:';
PRINT '-----------------------';
SELECT 
    COLUMN_NAME,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee' AND IS_NULLABLE = 'NO';
PRINT '';

-- 3. Check PRIMARY KEY
PRINT '3. Primary Key:';
PRINT '--------------';
SELECT 
    COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE TABLE_NAME = 'Employee' 
  AND CONSTRAINT_NAME LIKE 'PK%';
PRINT '';

-- 4. Check FOREIGN KEYS
PRINT '4. Foreign Keys:';
PRINT '---------------';
SELECT 
    fk.name AS 'FK Name',
    OBJECT_NAME(fk.parent_object_id) AS 'Table',
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS 'Column',
    OBJECT_NAME(fk.referenced_object_id) AS 'Referenced Table',
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS 'Referenced Column'
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('Employee');
PRINT '';

-- 5. Check UNIQUE constraints
PRINT '5. Unique Constraints:';
PRINT '---------------------';
SELECT 
    kc.name AS 'Constraint Name',
    c.name AS 'Column Name'
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id 
    AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id 
    AND ic.column_id = c.column_id
WHERE kc.parent_object_id = OBJECT_ID('Employee')
  AND kc.type = 'UQ';
PRINT '';

-- 6. Check CHECK constraints
PRINT '6. Check Constraints:';
PRINT '--------------------';
SELECT 
    cc.name AS 'Constraint Name',
    cc.definition AS 'Check Definition'
FROM sys.check_constraints cc
WHERE cc.parent_object_id = OBJECT_ID('Employee');

IF @@ROWCOUNT = 0
    PRINT 'No CHECK constraints found';
PRINT '';

-- 7. Check Department table values
PRINT '7. Department Table Values:';
PRINT '--------------------------';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Department')
BEGIN
    SELECT DeptID, DeptName FROM Department;
END
ELSE
BEGIN
    PRINT '? Department table does NOT exist!';
END
PRINT '';

-- 8. Test each field individually
PRINT '8. Field Validation Tests:';
PRINT '-------------------------';

-- Test Empid
BEGIN TRY
    DECLARE @TestEmpid VARCHAR(20) = '2699998';
    DELETE FROM Employee WHERE Empid = @TestEmpid;
    INSERT INTO Employee (Empid) VALUES (@TestEmpid);
    DELETE FROM Employee WHERE Empid = @TestEmpid;
    PRINT '? Empid: Can be NULL or has default';
END TRY
BEGIN CATCH
    PRINT '? Empid: ' + ERROR_MESSAGE();
END CATCH

-- Test EmployeeName
BEGIN TRY
    DECLARE @TestEmpid2 VARCHAR(20) = '2699997';
    DELETE FROM Employee WHERE Empid = @TestEmpid2;
    INSERT INTO Employee (Empid, EmployeeName) VALUES (@TestEmpid2, NULL);
    DELETE FROM Employee WHERE Empid = @TestEmpid2;
    PRINT '? EmployeeName: Can be NULL';
END TRY
BEGIN CATCH
    PRINT '? EmployeeName: ' + ERROR_MESSAGE();
END CATCH

-- Test DeptId
BEGIN TRY
    DECLARE @TestEmpid3 VARCHAR(20) = '2699996';
    DELETE FROM Employee WHERE Empid = @TestEmpid3;
    INSERT INTO Employee (Empid, EmployeeName, DeptId) VALUES (@TestEmpid3, 'Test', NULL);
    DELETE FROM Employee WHERE Empid = @TestEmpid3;
    PRINT '? DeptId: Can be NULL';
END TRY
BEGIN CATCH
    PRINT '? DeptId: ' + ERROR_MESSAGE();
END CATCH

-- Test DeptId with invalid value
BEGIN TRY
    DECLARE @TestEmpid4 VARCHAR(20) = '2699995';
    DELETE FROM Employee WHERE Empid = @TestEmpid4;
    INSERT INTO Employee (Empid, EmployeeName, DeptId) VALUES (@TestEmpid4, 'Test', 'INVALID');
    DELETE FROM Employee WHERE Empid = @TestEmpid4;
    PRINT '? DeptId: No foreign key constraint OR value exists';
END TRY
BEGIN CATCH
    PRINT '? DeptId Foreign Key: ' + ERROR_MESSAGE();
    PRINT '   ? DeptId must be one of: DEPT01, DEPT02, DEPT03';
END CATCH

-- Test Pan
BEGIN TRY
    DECLARE @TestEmpid5 VARCHAR(20) = '2699994';
    DELETE FROM Employee WHERE Empid = @TestEmpid5;
    INSERT INTO Employee (Empid, EmployeeName, DeptId, Pan) VALUES (@TestEmpid5, 'Test', 'DEPT01', NULL);
    DELETE FROM Employee WHERE Empid = @TestEmpid5;
    PRINT '? Pan: Can be NULL';
END TRY
BEGIN CATCH
    PRINT '? Pan: ' + ERROR_MESSAGE();
END CATCH

PRINT '';

-- 9. Full insert test
PRINT '9. Full Employee Insert Test:';
PRINT '-----------------------------';
BEGIN TRY
    BEGIN TRANSACTION;
    
    DECLARE @TestEmpidFull VARCHAR(20) = '2699999';
    DECLARE @TestPanFull VARCHAR(10) = 'TEST12345Z';
    
    -- Clean up
    DELETE FROM UserLogin WHERE ReferenceID = @TestEmpidFull;
    DELETE FROM Employee WHERE Empid = @TestEmpidFull;
    
    -- Try full insert
    INSERT INTO Employee (Empid, EmployeeName, DeptId, Pan)
    VALUES (@TestEmpidFull, 'Test Employee', 'DEPT01', @TestPanFull);
    
    PRINT '? SUCCESS! All fields accepted';
    
    -- Show inserted data
    SELECT * FROM Employee WHERE Empid = @TestEmpidFull;
    
    -- Clean up
    DELETE FROM Employee WHERE Empid = @TestEmpidFull;
    
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '? FAILED: ' + ERROR_MESSAGE();
    PRINT '';
    PRINT 'This is the exact error preventing employee registration!';
END CATCH
PRINT '';

PRINT '========================================';
PRINT 'CONSTRAINT CHECK COMPLETE';
PRINT '========================================';
PRINT '';

-- Summary
PRINT 'SUMMARY:';
PRINT '-------';
PRINT 'Look for ? errors above to identify the problem';
PRINT '';
PRINT 'Common Issues:';
PRINT '1. EmployeeName NOT NULL ? Cannot be empty';
PRINT '2. DeptId Foreign Key ? Must exist in Department table';
PRINT '3. DeptId NOT NULL ? Cannot be NULL';
PRINT '4. Pan UNIQUE constraint ? Must be unique or NULL';
PRINT '';

GO
