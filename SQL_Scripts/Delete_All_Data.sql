-- ============================================================
-- DELETE ALL DATA FROM BANKING DATABASE
-- ============================================================
-- WARNING: This will DELETE all data from all tables!
-- BACKUP your database before running this script!
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'STARTING DATA DELETION PROCESS';
PRINT '========================================';
PRINT '';

-- Step 1: Disable all foreign key constraints
PRINT 'Step 1: Disabling all foreign key constraints...';
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
PRINT 'Foreign key constraints disabled.';
PRINT '';

-- Step 2: Delete data from all tables (child tables first)
PRINT 'Step 2: Deleting data from all tables...';
PRINT '';

-- Transaction table (no dependencies)
PRINT '  - Deleting from SavingsTransaction...';
DELETE FROM SavingsTransaction;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Account sub-type tables (depend on Account)
PRINT '  - Deleting from SavingsAccount...';
DELETE FROM SavingsAccount;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

PRINT '  - Deleting from FixedDepositAccount...';
DELETE FROM FixedDepositAccount;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

PRINT '  - Deleting from LoanAccount...';
DELETE FROM LoanAccount;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Base Account table (depends on Customer)
PRINT '  - Deleting from Account...';
DELETE FROM Account;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- UserLogin table (depends on Customer, Employee, Manager)
PRINT '  - Deleting from UserLogin...';
DELETE FROM UserLogin;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

-- Entity tables (no dependencies)
PRINT '  - Deleting from Customer...';
DELETE FROM Customer;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

PRINT '  - Deleting from Employee...';
DELETE FROM Employee;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

PRINT '  - Deleting from Manager...';
DELETE FROM Manager;
PRINT '    Rows deleted: ' + CAST(@@ROWCOUNT AS VARCHAR(10));

PRINT '';

-- Step 3: Reset IDENTITY columns (if any)
PRINT 'Step 3: Resetting IDENTITY columns...';
DBCC CHECKIDENT ('SavingsTransaction', RESEED, 0);
PRINT '  - SavingsTransaction IDENTITY reset to 0';
PRINT '';

-- Step 4: Re-enable all foreign key constraints
PRINT 'Step 4: Re-enabling all foreign key constraints...';
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
PRINT 'Foreign key constraints re-enabled.';
PRINT '';

-- Step 5: Verify all tables are empty
PRINT 'Step 5: Verification - Checking row counts...';
PRINT '';
SELECT 'SavingsTransaction' AS TableName, COUNT(*) AS RowCount FROM SavingsTransaction
UNION ALL
SELECT 'SavingsAccount', COUNT(*) FROM SavingsAccount
UNION ALL
SELECT 'FixedDepositAccount', COUNT(*) FROM FixedDepositAccount
UNION ALL
SELECT 'LoanAccount', COUNT(*) FROM LoanAccount
UNION ALL
SELECT 'Account', COUNT(*) FROM Account
UNION ALL
SELECT 'UserLogin', COUNT(*) FROM UserLogin
UNION ALL
SELECT 'Customer', COUNT(*) FROM Customer
UNION ALL
SELECT 'Employee', COUNT(*) FROM Employee
UNION ALL
SELECT 'Manager', COUNT(*) FROM Manager;

PRINT '';
PRINT '========================================';
PRINT 'DATA DELETION COMPLETE!';
PRINT 'All tables are now empty.';
PRINT '========================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Create a new Manager account (see below)';
PRINT '2. Login and start fresh!';
PRINT '';

-- Optional: Insert a default manager account
PRINT '========================================';
PRINT 'OPTIONAL: Creating Default Manager Account';
PRINT '========================================';
PRINT '';

-- Insert Manager
INSERT INTO Manager (ManagerId, ManagerName, Pan)
VALUES ('MGR001', 'System Admin', 'ADMN0001');
PRINT 'Manager created: MGR001';

-- Insert UserLogin for Manager (plain password: Dummy)
INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('USER0001', 'admin', 'Dummy', 'MANAGER', 'MGR001');
PRINT 'UserLogin created: admin / Dummy';

PRINT '';
PRINT '========================================';
PRINT 'DEFAULT MANAGER ACCOUNT CREATED';
PRINT '========================================';
PRINT 'Username: admin';
PRINT 'Password: Dummy';
PRINT 'Manager ID: MGR001';
PRINT '========================================';
PRINT '';
PRINT 'You can now login with these credentials!';
PRINT '';

GO
