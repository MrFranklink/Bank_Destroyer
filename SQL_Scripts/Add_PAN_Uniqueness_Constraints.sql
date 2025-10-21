-- ============================================
-- PAN Uniqueness Constraints
-- Add database-level constraints for PAN uniqueness
-- ============================================
-- This provides an additional safety layer beyond application logic
-- ============================================

-- Step 1: Check for existing duplicate PANs in Customer table
SELECT Pan, COUNT(*) as Count, STRING_AGG(Custid, ', ') as CustomerIDs
FROM Customer
GROUP BY Pan
HAVING COUNT(*) > 1;
-- If this returns rows, you have duplicates that need to be resolved first!

-- Step 2: Check for existing duplicate PANs in Employee table
SELECT Pan, COUNT(*) as Count, STRING_AGG(Empid, ', ') as EmployeeIDs
FROM Employee
GROUP BY Pan
HAVING COUNT(*) > 1;
-- If this returns rows, you have duplicates that need to be resolved first!

-- Step 3: Check for PANs that exist in BOTH Customer and Employee tables
SELECT 
    c.Pan,
    c.Custid,
    c.Custname AS CustomerName,
    e.Empid,
    e.EmployeeName
FROM Customer c
INNER JOIN Employee e ON c.Pan = e.Pan;
-- If this returns rows, same person is both customer and employee!

-- ============================================
-- ONLY RUN THE FOLLOWING IF STEPS 1-3 RETURN NO ROWS
-- ============================================

-- Step 4: Add UNIQUE constraint on Customer.Pan
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'UQ_Customer_Pan' 
    AND object_id = OBJECT_ID('Customer')
)
BEGIN
    ALTER TABLE Customer
    ADD CONSTRAINT UQ_Customer_Pan UNIQUE (Pan);
    PRINT 'Unique constraint added to Customer.Pan';
END
ELSE
BEGIN
    PRINT 'Unique constraint already exists on Customer.Pan';
END;

-- Step 5: Add UNIQUE constraint on Employee.Pan
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'UQ_Employee_Pan' 
    AND object_id = OBJECT_ID('Employee')
)
BEGIN
    ALTER TABLE Employee
    ADD CONSTRAINT UQ_Employee_Pan UNIQUE (Pan);
    PRINT 'Unique constraint added to Employee.Pan';
END
ELSE
BEGIN
    PRINT 'Unique constraint already exists on Employee.Pan';
END;

-- Step 6: Verify constraints were added
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    type_desc AS ConstraintType
FROM sys.objects
WHERE type_desc = 'UNIQUE_CONSTRAINT'
AND name LIKE 'UQ_%_Pan';

-- ============================================
-- OPTIONAL: Add indexes for better performance
-- ============================================

-- Step 7: Add non-clustered index on Customer.Pan (if not exists)
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Customer_Pan' 
    AND object_id = OBJECT_ID('Customer')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Customer_Pan 
    ON Customer(Pan);
    PRINT 'Index created on Customer.Pan';
END
ELSE
BEGIN
    PRINT 'Index already exists on Customer.Pan';
END;

-- Step 8: Add non-clustered index on Employee.Pan (if not exists)
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Employee_Pan' 
    AND object_id = OBJECT_ID('Employee')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Employee_Pan 
    ON Employee(Pan);
    PRINT 'Index created on Employee.Pan';
END
ELSE
BEGIN
    PRINT 'Index already exists on Employee.Pan';
END;

-- ============================================
-- Testing the Constraints
-- ============================================

-- Step 9: Test duplicate customer PAN (should fail)
BEGIN TRY
    INSERT INTO Customer (Custid, Custname, Pan)
    VALUES ('TEST001', 'Test Customer', 
        (SELECT TOP 1 Pan FROM Customer));
    PRINT 'ERROR: Duplicate PAN was allowed! Constraint not working.';
    ROLLBACK;
END TRY
BEGIN CATCH
    PRINT 'SUCCESS: Duplicate PAN blocked by constraint.';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
END CATCH;

-- Step 10: Test duplicate employee PAN (should fail)
BEGIN TRY
    INSERT INTO Employee (Empid, EmployeeName, Pan)
    VALUES ('TEST001', 'Test Employee', 
        (SELECT TOP 1 Pan FROM Employee));
    PRINT 'ERROR: Duplicate PAN was allowed! Constraint not working.';
    ROLLBACK;
END TRY
BEGIN CATCH
    PRINT 'SUCCESS: Duplicate PAN blocked by constraint.';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
END CATCH;

-- ============================================
-- Cleanup Test Data (if any)
-- ============================================
DELETE FROM Customer WHERE Custid = 'TEST001';
DELETE FROM Employee WHERE Empid = 'TEST001';

-- ============================================
-- NOTES:
-- ============================================
-- 1. Database constraints provide an extra safety layer
-- 2. Application validation (C# code) runs FIRST
-- 3. Database constraints are the LAST line of defense
-- 4. Unique constraints ensure data integrity even if app logic fails
-- 5. Indexes improve query performance when checking PAN existence

-- ============================================
-- TO REMOVE CONSTRAINTS (if needed):
-- ============================================
-- ALTER TABLE Customer DROP CONSTRAINT UQ_Customer_Pan;
-- ALTER TABLE Employee DROP CONSTRAINT UQ_Employee_Pan;
-- DROP INDEX IX_Customer_Pan ON Customer;
-- DROP INDEX IX_Employee_Pan ON Employee;
