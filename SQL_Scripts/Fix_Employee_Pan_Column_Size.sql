-- ============================================================
-- FIX EMPLOYEE PAN COLUMN SIZE - COMPLETE FIX
-- ============================================================
-- Updates Employee.Pan from VARCHAR(8) to VARCHAR(10)
-- Also updates Customer.Pan and Manager.Pan if needed
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'FIXING PAN COLUMN SIZE TO VARCHAR(10)';
PRINT '========================================';
PRINT '';

-- Step 1: Check current sizes
PRINT 'Step 1: Current PAN column sizes:';
PRINT '---------------------------------';
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH as 'Current Length'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Pan'
ORDER BY TABLE_NAME;
PRINT '';

-- Step 2: Drop UNIQUE constraints (if exist) before altering
PRINT 'Step 2: Dropping UNIQUE constraints temporarily...';
PRINT '------------------------------------------------';

DECLARE @sql NVARCHAR(MAX) = N'';

-- Find all UNIQUE constraints on Pan columns
SELECT @sql = @sql + 
    'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + 
    QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.key_constraints
WHERE type = 'UQ'
  AND parent_object_id IN (OBJECT_ID('Employee'), OBJECT_ID('Customer'), OBJECT_ID('Manager'))
  AND EXISTS (
      SELECT 1 
      FROM sys.index_columns ic
      JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
      WHERE ic.object_id = sys.key_constraints.parent_object_id
        AND ic.index_id = sys.key_constraints.unique_index_id
        AND c.name = 'Pan'
  );

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping constraints:';
    PRINT @sql;
    EXEC sp_executesql @sql;
    PRINT 'Constraints dropped ?';
END
ELSE
BEGIN
    PRINT 'No UNIQUE constraints found on Pan columns';
END
PRINT '';

-- Step 3: Update column sizes
PRINT 'Step 3: Updating Pan columns to VARCHAR(10)...';
PRINT '----------------------------------------------';

-- Employee.Pan
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Employee' AND COLUMN_NAME = 'Pan')
BEGIN
    ALTER TABLE Employee ALTER COLUMN Pan VARCHAR(10) NULL;
    PRINT '? Employee.Pan updated to VARCHAR(10)';
END

-- Customer.Pan
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Customer' AND COLUMN_NAME = 'Pan')
BEGIN
    ALTER TABLE Customer ALTER COLUMN Pan VARCHAR(10) NULL;
    PRINT '? Customer.Pan updated to VARCHAR(10)';
END

-- Manager.Pan
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manager' AND COLUMN_NAME = 'Pan')
BEGIN
    ALTER TABLE Manager ALTER COLUMN Pan VARCHAR(10) NULL;
    PRINT '? Manager.Pan updated to VARCHAR(10)';
END
PRINT '';

-- Step 4: Recreate UNIQUE constraints
PRINT 'Step 4: Recreating UNIQUE constraints...';
PRINT '---------------------------------------';

-- Employee.Pan
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Employee_Pan')
BEGIN
    ALTER TABLE Employee ADD CONSTRAINT UQ_Employee_Pan UNIQUE (Pan);
    PRINT '? Created UQ_Employee_Pan';
END

-- Customer.Pan
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Customer_Pan')
BEGIN
    ALTER TABLE Customer ADD CONSTRAINT UQ_Customer_Pan UNIQUE (Pan);
    PRINT '? Created UQ_Customer_Pan';
END

-- Manager.Pan (if column exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Manager' AND COLUMN_NAME = 'Pan')
AND NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Manager_Pan')
BEGIN
    ALTER TABLE Manager ADD CONSTRAINT UQ_Manager_Pan UNIQUE (Pan);
    PRINT '? Created UQ_Manager_Pan';
END
PRINT '';

-- Step 5: Verify changes
PRINT 'Step 5: Verifying updated column sizes...';
PRINT '----------------------------------------';
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH as 'New Length'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Pan'
ORDER BY TABLE_NAME;
PRINT '';

-- Step 6: Test insert with 10-character PAN
PRINT 'Step 6: Testing 10-character PAN insert...';
PRINT '-----------------------------------------';

BEGIN TRY
    BEGIN TRANSACTION;
    
    DECLARE @TestEmpId VARCHAR(20) = '2699999';
    DECLARE @TestPan VARCHAR(10) = 'ABCDE1234F'; -- 10 characters
    
    -- Clean up
    DELETE FROM UserLogin WHERE ReferenceID = @TestEmpId;
    DELETE FROM Employee WHERE Empid = @TestEmpId;
    
    -- Try insert with 10-char PAN
    INSERT INTO Employee (Empid, EmployeeName, DeptId, Pan)
    VALUES (@TestEmpId, 'Test Employee', 'DEPT01', @TestPan);
    
    PRINT '? SUCCESS! 10-character PAN accepted';
    PRINT 'Pan value: ' + @TestPan + ' (length: ' + CAST(LEN(@TestPan) AS VARCHAR) + ')';
    
    -- Clean up
    DELETE FROM Employee WHERE Empid = @TestEmpId;
    
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '? FAILED: ' + ERROR_MESSAGE();
END CATCH
PRINT '';

PRINT '========================================';
PRINT 'PAN COLUMN FIX COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Close Visual Studio';
PRINT '2. Delete DB\Model1.edmx.diagram (if exists)';
PRINT '3. Open Model1.edmx in Visual Studio';
PRINT '4. Right-click design surface ? "Update Model from Database"';
PRINT '5. Select "Employee", "Customer", "Manager" tables';
PRINT '6. Click "Finish" to refresh the model';
PRINT '7. Save Model1.edmx';
PRINT '8. Rebuild solution (Ctrl+Shift+B)';
PRINT '9. Try registering employee again';
PRINT '';
PRINT 'Or simply REBUILD the Entity Framework model:';
PRINT '1. Delete Model1.edmx';
PRINT '2. Add new ADO.NET Entity Data Model';
PRINT '3. Select all tables';
PRINT '4. Rebuild solution';
PRINT '';

GO
