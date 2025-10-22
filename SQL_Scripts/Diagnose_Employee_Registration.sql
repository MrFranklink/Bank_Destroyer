-- ============================================================
-- EMPLOYEE REGISTRATION DIAGNOSTIC SCRIPT
-- ============================================================
-- Run this to check if your Employee table is properly configured
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'EMPLOYEE REGISTRATION DIAGNOSTICS';
PRINT '========================================';
PRINT '';

-- 1. Check Employee table structure
PRINT '1. Employee Table Structure:';
PRINT '----------------------------';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee'
ORDER BY ORDINAL_POSITION;
PRINT '';

-- 2. Check if Pan column exists
PRINT '2. Pan Column Check:';
PRINT '-------------------';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employee' AND COLUMN_NAME = 'Pan')
BEGIN
    PRINT '? Pan column EXISTS';
    SELECT 
        DATA_TYPE as 'Data Type',
        CHARACTER_MAXIMUM_LENGTH as 'Max Length',
        IS_NULLABLE as 'Nullable'
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Employee' AND COLUMN_NAME = 'Pan';
END
ELSE
BEGIN
    PRINT '? Pan column DOES NOT EXIST';
    PRINT 'Run this to add it:';
    PRINT 'ALTER TABLE Employee ADD Pan VARCHAR(10) NULL;';
END
PRINT '';

-- 3. Check Foreign Key constraints
PRINT '3. Foreign Key Constraints:';
PRINT '---------------------------';
SELECT 
    fk.name AS 'Constraint Name',
    tp.name AS 'Parent Table',
    cp.name AS 'Parent Column',
    tr.name AS 'Referenced Table',
    cr.name AS 'Referenced Column'
FROM sys.foreign_keys AS fk
INNER JOIN sys.tables AS tp ON fk.parent_object_id = tp.object_id
INNER JOIN sys.tables AS tr ON fk.referenced_object_id = tr.object_id
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns AS cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
INNER JOIN sys.columns AS cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
WHERE tp.name = 'Employee';
PRINT '';

-- 4. Check Department table exists
PRINT '4. Department Table Check:';
PRINT '-------------------------';
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Department')
BEGIN
    PRINT '? Department table EXISTS';
    SELECT DeptID, DeptName FROM Department;
END
ELSE
BEGIN
    PRINT '? Department table DOES NOT EXIST';
    PRINT 'Employee registration requires Department table!';
END
PRINT '';

-- 5. Check existing employees
PRINT '5. Existing Employees:';
PRINT '---------------------';
IF EXISTS (SELECT * FROM Employee)
BEGIN
    SELECT 
        Empid,
        EmployeeName,
        DeptId,
        Pan,
        LEN(Pan) as 'PAN Length'
    FROM Employee;
    PRINT '';
    PRINT 'Total Employees: ';
    SELECT COUNT(*) as 'Count' FROM Employee;
END
ELSE
BEGIN
    PRINT 'No employees found (table is empty)';
END
PRINT '';

-- 6. Check next Employee ID
PRINT '6. Next Employee ID:';
PRINT '-------------------';
DECLARE @LastEmpId VARCHAR(20);
SELECT @LastEmpId = MAX(Empid) FROM Employee WHERE Empid LIKE '26%';

IF @LastEmpId IS NULL
BEGIN
    PRINT 'Next Employee ID will be: 2600001';
END
ELSE
BEGIN
    PRINT 'Last Employee ID: ' + @LastEmpId;
    DECLARE @NextNum INT = CAST(SUBSTRING(@LastEmpId, 3, 5) AS INT) + 1;
    PRINT 'Next Employee ID will be: 26' + RIGHT('00000' + CAST(@NextNum AS VARCHAR(5)), 5);
END
PRINT '';

-- 7. Check UserLogin table
PRINT '7. UserLogin Table Check:';
PRINT '------------------------';
SELECT 
    UserID,
    UserName,
    Role,
    ReferenceID
FROM UserLogin
WHERE Role = 'EMPLOYEE';
PRINT '';

-- 8. Check for PAN duplicates
PRINT '8. Duplicate PAN Check:';
PRINT '----------------------';
PRINT 'Employees with duplicate PANs:';
SELECT Pan, COUNT(*) as 'Count'
FROM Employee
WHERE Pan IS NOT NULL
GROUP BY Pan
HAVING COUNT(*) > 1;

IF @@ROWCOUNT = 0
    PRINT 'No duplicate PANs found ?';
PRINT '';

-- 9. Check PAN uniqueness constraints
PRINT '9. PAN Uniqueness Constraints:';
PRINT '-----------------------------';
SELECT 
    OBJECT_NAME(parent_object_id) AS 'Table',
    name AS 'Constraint Name',
    type_desc AS 'Type'
FROM sys.key_constraints
WHERE parent_object_id = OBJECT_ID('Employee')
  AND type = 'UQ'
  AND EXISTS (
      SELECT 1 
      FROM sys.index_columns ic
      JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
      WHERE ic.object_id = sys.key_constraints.parent_object_id
        AND ic.index_id = sys.key_constraints.unique_index_id
        AND c.name = 'Pan'
  );

IF @@ROWCOUNT = 0
    PRINT 'No UNIQUE constraint on Pan column';
ELSE
    PRINT 'UNIQUE constraint exists on Pan column ?';
PRINT '';

-- 10. Test data insert
PRINT '10. Test Employee Insert:';
PRINT '------------------------';
PRINT 'Attempting to insert test employee...';

BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Generate test ID
    DECLARE @TestEmpId VARCHAR(20) = '2699999';
    DECLARE @TestPan VARCHAR(10) = 'TEST12345Z';
    
    -- Clean up test data if exists
    DELETE FROM UserLogin WHERE ReferenceID = @TestEmpId;
    DELETE FROM Employee WHERE Empid = @TestEmpId;
    
    -- Try insert
    INSERT INTO Employee (Empid, EmployeeName, DeptId, Pan)
    VALUES (@TestEmpId, 'Test Employee', 'DEPT01', @TestPan);
    
    PRINT '? Test insert SUCCESSFUL';
    PRINT 'Employee table can accept new records';
    
    -- Clean up
    DELETE FROM Employee WHERE Empid = @TestEmpId;
    
    ROLLBACK TRANSACTION;
    PRINT 'Test data cleaned up';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '? Test insert FAILED';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'This is why employee registration is failing!';
END CATCH
PRINT '';

PRINT '========================================';
PRINT 'DIAGNOSTIC COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'COMMON ISSUES:';
PRINT '1. Pan column missing ? Add with: ALTER TABLE Employee ADD Pan VARCHAR(10) NULL;';
PRINT '2. Department table missing ? Create Department table';
PRINT '3. Foreign key constraint error ? Check Department.DeptID values';
PRINT '4. Duplicate PAN ? Check for existing PAN in database';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Fix any ? errors shown above';
PRINT '2. Run the application and try registering employee again';
PRINT '3. Check Output window in Visual Studio for detailed error message';
PRINT '';

GO
