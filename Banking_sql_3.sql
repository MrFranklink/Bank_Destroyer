-- ============================================================
-- UPDATE PAN FORMAT TO REAL INDIAN PAN (10 CHARACTERS)
-- ============================================================
-- Changes PAN column from VARCHAR(8) to VARCHAR(10)
-- Old Format: ABCD1234 (4 letters + 4 digits)
-- New Format: ABCDE1234F (5 letters + 4 digits + 1 letter)
-- Handles UNIQUE constraints properly
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'UPDATING PAN FORMAT TO REAL INDIAN PAN';
PRINT '========================================';
PRINT '';

-- Step 1: Check current PAN column sizes
PRINT 'Step 1: Current PAN column information...';
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Pan'
ORDER BY TABLE_NAME;
PRINT '';

-- Step 2: Backup existing data (optional - comment out if not needed)
PRINT 'Step 2: Creating backup of PAN data...';

-- Drop backup tables if they already exist
IF OBJECT_ID('Customer_PAN_Backup', 'U') IS NOT NULL DROP TABLE Customer_PAN_Backup;
IF OBJECT_ID('Employee_PAN_Backup', 'U') IS NOT NULL DROP TABLE Employee_PAN_Backup;
IF OBJECT_ID('Manager_PAN_Backup', 'U') IS NOT NULL DROP TABLE Manager_PAN_Backup;

SELECT * INTO Customer_PAN_Backup FROM Customer;
SELECT * INTO Employee_PAN_Backup FROM Employee;
SELECT * INTO Manager_PAN_Backup FROM Manager;
PRINT 'Backup created!';
PRINT '';

-- Step 3: Check for existing PAN values that don't match new format
PRINT 'Step 3: Checking existing PAN values...';
PRINT 'Customer PANs with length != 10:';
SELECT Custid, Custname, Pan, LEN(Pan) as PAN_Length 
FROM Customer 
WHERE LEN(Pan) != 10 OR Pan IS NULL;

PRINT 'Employee PANs with length != 10:';
SELECT Empid, EmployeeName, Pan, LEN(Pan) as PAN_Length 
FROM Employee 
WHERE LEN(Pan) != 10 OR Pan IS NULL;

PRINT 'Manager PANs with length != 10:';
SELECT ManagerId, ManagerName, Pan, LEN(Pan) as PAN_Length 
FROM Manager 
WHERE LEN(Pan) != 10 OR Pan IS NULL;
PRINT '';

-- Step 4: Find and drop existing UNIQUE constraints on Pan columns
PRINT 'Step 4: Finding and dropping existing UNIQUE constraints on Pan columns...';

DECLARE @sql NVARCHAR(MAX) = N'';

-- Find all UNIQUE constraints on Pan column
SELECT @sql = @sql + 
    'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + 
    QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    ' DROP CONSTRAINT ' + QUOTENAME(name) + ''
FROM sys.key_constraints
WHERE type = 'UQ' -- UNIQUE constraint
  AND parent_object_id IN (
      OBJECT_ID('Customer'), 
      OBJECT_ID('Employee'), 
      OBJECT_ID('Manager')
  )
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
    PRINT 'Dropping UNIQUE constraints:';
    PRINT @sql;
    EXEC sp_executesql @sql;
    PRINT 'UNIQUE constraints dropped successfully.';
END
ELSE
BEGIN
    PRINT 'No UNIQUE constraints found on Pan columns.';
END
PRINT '';

-- Step 5: Update column size from VARCHAR(8) to VARCHAR(10)
PRINT 'Step 5: Updating PAN column size to VARCHAR(10)...';

-- Customer table
ALTER TABLE Customer
ALTER COLUMN Pan VARCHAR(10) NULL;
PRINT 'Customer.Pan updated to VARCHAR(10)';

-- Employee table
ALTER TABLE Employee
ALTER COLUMN Pan VARCHAR(10) NULL;
PRINT 'Employee.Pan updated to VARCHAR(10)';

-- Manager table
ALTER TABLE Manager
ALTER COLUMN Pan VARCHAR(10) NULL;
PRINT 'Manager.Pan updated to VARCHAR(10)';
PRINT '';

-- Step 6: Recreate UNIQUE constraints on Pan columns
PRINT 'Step 6: Recreating UNIQUE constraints on Pan columns...';

-- Customer.Pan UNIQUE constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.key_constraints 
    WHERE name = 'UQ_Customer_Pan' 
    AND parent_object_id = OBJECT_ID('Customer')
)
BEGIN
    ALTER TABLE Customer ADD CONSTRAINT UQ_Customer_Pan UNIQUE (Pan);
    PRINT 'Created UNIQUE constraint: UQ_Customer_Pan';
END

-- Employee.Pan UNIQUE constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.key_constraints 
    WHERE name = 'UQ_Employee_Pan' 
    AND parent_object_id = OBJECT_ID('Employee')
)
BEGIN
    ALTER TABLE Employee ADD CONSTRAINT UQ_Employee_Pan UNIQUE (Pan);
    PRINT 'Created UNIQUE constraint: UQ_Employee_Pan';
END

-- Manager.Pan UNIQUE constraint
IF NOT EXISTS (
    SELECT 1 FROM sys.key_constraints 
    WHERE name = 'UQ_Manager_Pan' 
    AND parent_object_id = OBJECT_ID('Manager')
)
BEGIN
    ALTER TABLE Manager ADD CONSTRAINT UQ_Manager_Pan UNIQUE (Pan);
    PRINT 'Created UNIQUE constraint: UQ_Manager_Pan';
END
PRINT '';

-- Step 7: Verify changes
PRINT 'Step 7: Verifying column updates...';
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH as New_Length
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Pan'
ORDER BY TABLE_NAME;
PRINT '';

PRINT 'Verifying UNIQUE constraints...';
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName,
    type_desc AS ConstraintType
FROM sys.key_constraints
WHERE parent_object_id IN (OBJECT_ID('Customer'), OBJECT_ID('Employee'), OBJECT_ID('Manager'))
  AND type = 'UQ'
ORDER BY TableName;
PRINT '';

-- Step 8: Sample valid PAN formats
PRINT 'Step 8: Sample valid PAN formats (Indian Standard):';
PRINT '  - ABCDE1234F (Individual)';
PRINT '  - BNZAA2318J (Individual)';
PRINT '  - AABCP1234C (Company)';
PRINT '  - AABCH1234F (Firm)';
PRINT '';
PRINT 'PAN Structure:';
PRINT '  Position 1-3: Alphabetic series (AAA-ZZZ)';
PRINT '  Position 4: Status (P=Person, C=Company, H=HUF, F=Firm, etc.)';
PRINT '  Position 5: First letter of surname/name';
PRINT '  Position 6-9: Sequential number (0000-9999)';
PRINT '  Position 10: Check digit (alphabet)';
PRINT '';

PRINT '========================================';
PRINT 'PAN FORMAT UPDATE COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'IMPORTANT NOTES:';
PRINT '1. All new PANs must be 10 characters (e.g., ABCDE1234F)';
PRINT '2. Old 8-character PANs will still work but should be updated';
PRINT '3. UNIQUE constraints have been recreated with standard names';
PRINT '4. Update Entity Framework model (.edmx) if needed';
PRINT '5. Update application validation to enforce 10-char format';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Update existing PAN values to 10-char format (see examples below)';
PRINT '2. Test PAN validation in application';
PRINT '3. Drop backup tables once confirmed:';
PRINT '   DROP TABLE Customer_PAN_Backup;';
PRINT '   DROP TABLE Employee_PAN_Backup;';
PRINT '   DROP TABLE Manager_PAN_Backup;';
PRINT '';
PRINT 'Example updates for existing data:';
PRINT '   UPDATE Customer SET Pan = ''ABCDE1234F'' WHERE Custid = ''MLA00001'';';
PRINT '   UPDATE Employee SET Pan = ''FGHIJ5678K'' WHERE Empid = ''2600001'';';
PRINT '   UPDATE Manager SET Pan = ''KLMNO9012P'' WHERE ManagerId = ''MGR001'';';
PRINT '';

GO


-- Check column sizes (should all be 10)
SELECT TABLE_NAME, CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE COLUMN_NAME = 'Pan';

-- Check UNIQUE constraints (should have 3 rows)
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName
FROM sys.key_constraints
WHERE type = 'UQ' 
  AND parent_object_id IN (
      OBJECT_ID('Customer'), 
      OBJECT_ID('Employee'), 
      OBJECT_ID('Manager')
  );

  UPDATE Manager SET Pan = 'ADMNO0001P' WHERE ManagerId = 'MGR001';


  Select * from Manager

  DROP TABLE Customer_PAN_Backup;
DROP TABLE Employee_PAN_Backup;
   DROP TABLE Manager_PAN_Backup;


   -- Check if Pan column exists in Employee table
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee' AND COLUMN_NAME = 'Pan';

Select * from Department

Delete from Department

USE Banking_Details;
GO

-- Update Employee.Pan column from VARCHAR(8) to VARCHAR(10)
ALTER TABLE Employee ALTER COLUMN Pan VARCHAR(10) NULL;

-- Verify
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Employee' AND COLUMN_NAME = 'Pan';

-- Should show: Pan | varchar | 10

PRINT 'Pan column updated to VARCHAR(10) ✅';
GO

select * from LoanAccount

Delete  from Account where AccountID='LN00001'


USE Banking_Details;
GO

-- Drop and recreate LoanTransaction
DROP TABLE IF EXISTS LoanTransaction;

CREATE TABLE LoanTransaction (
    Transactionno INT IDENTITY(1,1) PRIMARY KEY,
    Ln_accountid CHAR(7) NOT NULL,
    Emidate DATETIME,
    Amount DECIMAL(12,2),
    Outstanding DECIMAL(12,2),
    CONSTRAINT FK_LoanTransaction_LoanAccount 
    FOREIGN KEY (Ln_accountid) REFERENCES LoanAccount(Ln_accountid)
);

SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'LoanTransaction';

-- Run these in order:
1. SQL_Scripts/Create_FundTransfer_Table.sql
2. SQL_Scripts/Update_LoanTransaction_Table.sql