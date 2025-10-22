create Database Banking_Details
use Banking_Details

 CREATE TABLE Employee (
    Empid VARCHAR(20) PRIMARY KEY CHECK (LEFT(Empid, 2) = '26'),
    EmployeeName VARCHAR(20) NOT NULL,
    DeptId CHAR(6),
    Pan VARCHAR(8) NOT NULL CHECK (Pan NOT LIKE '%[^a-zA-Z0-9]%'),
    FOREIGN KEY (DeptId) REFERENCES Department(Deptid)
);

Select * from Employee

CREATE TABLE Customer (
    Custid CHAR(8) PRIMARY KEY,
    Custname VARCHAR(20) NOT NULL,
    DOB DATE CHECK (DATEDIFF(YEAR, DOB, GETDATE()) >= 18),
    Pan VARCHAR(20) NOT NULL CHECK (Pan NOT LIKE '%[^a-zA-Z0-9]%'),
    Address VARCHAR(100),
    PhoneNumber VARCHAR(15)
);

Select * from Customer


CREATE TABLE Department (
    Deptid CHAR(6) PRIMARY KEY,
    Deptname VARCHAR(20) NOT NULL UNIQUE
);



CREATE TABLE Account (
    AccountID CHAR(7) PRIMARY KEY,
    AccountType VARCHAR(15) CHECK (AccountType IN ('SAVING','FIXED-DEPOSIT','LOAN')),
    CustomerID CHAR(8),
    OpenedBy VARCHAR(20),
    OpenedByRole VARCHAR(10) CHECK (OpenedByRole IN ('EMPLOYEE','MANAGER')),
    OpenDate DATE NOT NULL,
    Status VARCHAR(10) CHECK (Status IN ('OPEN','CLOSED')),
    ClosedDate DATE,
    FOREIGN KEY (CustomerID) REFERENCES Customer(Custid),
    FOREIGN KEY (OpenedBy) REFERENCES Employee(Empid)
);



CREATE TABLE SavingsAccount (
    SBAccountID CHAR(7) PRIMARY KEY,
    Customerid CHAR(8) UNIQUE,
    Balance SMALLMONEY CHECK (Balance > 1000),
    FOREIGN KEY (SBAccountID) REFERENCES Account(AccountID),
    FOREIGN KEY (Customerid) REFERENCES Customer(Custid)
);



CREATE TABLE SavingsTransaction (
    Transactionid INT IDENTITY(1,1) PRIMARY KEY,  
    SBAccountID CHAR(7) NOT NULL,
    Transationdate DATETIME DEFAULT GETDATE(),
    Transactiontype VARCHAR(10) CHECK (Transactiontype IN ('DEPOSIT','WITHDRAW')),
    Amount DECIMAL(18,2) CHECK (Amount >= 100),
    CONSTRAINT FK_SavingsTransaction_SavingsAccount 
        FOREIGN KEY (SBAccountID) REFERENCES SavingsAccount(SBAccountID)
);



CREATE TABLE LoanAccount (
    [Ln-accountid] CHAR(7) PRIMARY KEY,
    Customer CHAR(8),
    [loan-amount] SMALLMONEY CHECK ([loan-amount] >= 10000),
    [Start-date] DATE NOT NULL CHECK ([Start-date] > GETDATE()),
    Tenure INT NOT NULL,
    [Ln-roi] DECIMAL(4,2) NOT NULL,
    Emi DECIMAL(12,2) CHECK (Emi > 1),
    FOREIGN KEY (Customer) REFERENCES Customer(Custid),
    FOREIGN KEY ([Ln-accountid]) REFERENCES Account(AccountID)
);



CREATE TABLE LoanTransaction (
    Transacitonno INT PRIMARY KEY IDENTITY(1,1),
    [Ln-account-id] CHAR(7),
    Emidate DATE CHECK (Emidate > GETDATE()),
    Amount SMALLMONEY CHECK (Amount > 0),
    Outstanding INT,
    FOREIGN KEY ([Ln-account-id]) REFERENCES LoanAccount([Ln-accountid])
);



CREATE TABLE UserLogin (
    UserID VARCHAR(20) PRIMARY KEY,
    UserName VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(15) CHECK (Role IN ('CUSTOMER','EMPLOYEE','MANAGER')),
    ReferenceID VARCHAR(8)
    --Trigger
);



CREATE TABLE Manager (
    ManagerID VARCHAR(8) PRIMARY KEY,
    ManagerName VARCHAR(50) NOT NULL,
    PAN CHAR(8) NOT NULL UNIQUE
);


CREATE TABLE FixedDepositAccount (
    FDAccountID CHAR(7) PRIMARY KEY,
    CustomerID CHAR(8),
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    FD_ROI DECIMAL(4,2) NOT NULL,
    Amount DECIMAL(12,2) CHECK (Amount >= 10000),
    MaturityAmount AS (Amount + (Amount * FD_ROI * DATEDIFF(DAY, StartDate, EndDate) / 36500.0)),
    FOREIGN KEY (FDAccountID) REFERENCES Account(AccountID),
    FOREIGN		KEY (CustomerID) REFERENCES Customer(Custid)
);

Select * from FixedDepositAccount


CREATE TABLE FDTransaction (
    TransactionID INT PRIMARY KEY,
    FDAccountID CHAR(7),
    TransactionType VARCHAR(15) CHECK (TransactionType = 'FORECLOSE'),
    Amount SMALLMONEY NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (FDAccountID) REFERENCES FixedDepositAccount(FDAccountID)
);

Select * From FDTransaction


INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U001', 'admin', 'admin123', 'MANAGER', NULL);
UPDATE UserLogin
SET Role = 'Manager'
WHERE UserID='U001';

INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U002', 'admin1', 'admin123', 'CUSTOMER', NULL);

INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U003', 'admin2', 'admin123', 'EMPLOYEE', NULL);

INSERT INTO Manager (ManagerID, ManagerName, PAN)
VALUES ('MGR001', 'Admin Manager', 'ABCD1234');

INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U001', 'admin', 'admin123', 'MANAGER', 'MGR001');

Select * from UserLogin
---Later work when we delete some emp it will delete for others and some trigger work we have to do



-- Check if Department table has any records
SELECT * FROM Department;

Select * from Account

-- If empty, insert a test department
INSERT INTO Department (Deptid, Deptname)
VALUES ('DEPT01', 'Sales Department');

INSERT INTO Department (Deptid, Deptname)
VALUES ('DEPT02', 'IT Department');

INSERT INTO Department (Deptid, Deptname)
VALUES ('DEPT03', 'HR Department');



-- =============================================
-- TRIGGERS FOR BANKING_DETAILS DATABASE
-- =============================================

USE Banking_Details;
GO

-- =============================================
-- Trigger 1: AUTO-POPULATE ReferenceID on INSERT
-- =============================================
CREATE OR ALTER TRIGGER dbo.trg_UserLogin_AutoPopulateReferenceID
ON dbo.UserLogin
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        -- Auto-populate ReferenceID for CUSTOMER
        -- Assumes UserID matches Custid OR we find by username pattern
        UPDATE ul
        SET ul.ReferenceID = COALESCE(
            -- Option 1: Try to match UserID with Custid
            (SELECT TOP 1 c.Custid FROM dbo.Customer c WHERE c.Custid = i.UserID),
            -- Option 2: If ReferenceID was manually provided, use it
            i.ReferenceID
        )
        FROM dbo.UserLogin AS ul
        INNER JOIN inserted AS i ON ul.UserID = i.UserID
        WHERE i.Role = 'CUSTOMER'
          AND ul.ReferenceID IS NULL;

        -- Auto-populate ReferenceID for EMPLOYEE
        UPDATE ul
        SET ul.ReferenceID = COALESCE(
            (SELECT TOP 1 e.Empid FROM dbo.Employee e WHERE e.Empid = i.UserID),
            i.ReferenceID
        )
        FROM dbo.UserLogin AS ul
        INNER JOIN inserted AS i ON ul.UserID = i.UserID
        WHERE i.Role = 'EMPLOYEE'
          AND ul.ReferenceID IS NULL;

        -- Auto-populate ReferenceID for MANAGER
        UPDATE ul
        SET ul.ReferenceID = COALESCE(
            (SELECT TOP 1 m.ManagerID FROM dbo.Manager m WHERE m.ManagerID = i.UserID),
            i.ReferenceID
        )
        FROM dbo.UserLogin AS ul
        INNER JOIN inserted AS i ON ul.UserID = i.UserID
        WHERE i.Role = 'MANAGER'
          AND ul.ReferenceID IS NULL;

        -- Validate that ReferenceID was successfully populated
        IF EXISTS (
            SELECT 1 FROM dbo.UserLogin ul
            INNER JOIN inserted i ON ul.UserID = i.UserID
            WHERE ul.ReferenceID IS NULL AND ul.Role IN ('CUSTOMER', 'EMPLOYEE', 'MANAGER')
        )
        BEGIN
            RAISERROR('ReferenceID could not be auto-populated. Ensure the corresponding Customer/Employee/Manager record exists with matching ID.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        THROW;
    END CATCH;
END;
GO

-- =============================================
-- Trigger 2: Validate ReferenceID on UPDATE
-- =============================================
CREATE OR ALTER TRIGGER dbo.trg_UserLogin_ValidateReferenceOnUpdate
ON dbo.UserLogin
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        IF UPDATE(ReferenceID) OR UPDATE(Role)
        BEGIN
            -- Validate CUSTOMER ReferenceID exists
            IF EXISTS (
                SELECT 1 FROM inserted 
                WHERE Role = 'CUSTOMER' 
                AND ReferenceID IS NOT NULL
                AND NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE Custid = ReferenceID)
            )
            BEGIN
                RAISERROR('Invalid ReferenceID for CUSTOMER role.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END;
            
            -- Validate EMPLOYEE ReferenceID exists
            IF EXISTS (
                SELECT 1 FROM inserted 
                WHERE Role = 'EMPLOYEE' 
                AND ReferenceID IS NOT NULL
                AND NOT EXISTS (SELECT 1 FROM dbo.Employee WHERE Empid = ReferenceID)
            )
            BEGIN
                RAISERROR('Invalid ReferenceID for EMPLOYEE role.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END;
            
            -- Validate MANAGER ReferenceID exists
            IF EXISTS (
                SELECT 1 FROM inserted 
                WHERE Role = 'MANAGER' 
                AND ReferenceID IS NOT NULL
                AND NOT EXISTS (SELECT 1 FROM dbo.Manager WHERE ManagerID = ReferenceID)
            )
            BEGIN
                RAISERROR('Invalid ReferenceID for MANAGER role.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN;
            END;
        END;
    END TRY
    BEGIN CATCH
        DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
        -- Optional: log @msg to an audit/error table
        THROW; -- rethrow the original error
    END CATCH;
END;
GO

-- =============================================
-- Trigger 3: Cascade delete UserLogin when Customer is deleted
-- =============================================
CREATE OR ALTER TRIGGER dbo.trg_Customer_CascadeDeleteUserLogin
ON dbo.Customer
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DELETE ul
        FROM dbo.UserLogin ul
        INNER JOIN deleted d ON ul.ReferenceID = d.Custid
        WHERE ul.Role = 'CUSTOMER';
    END TRY
    BEGIN CATCH
        DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
        -- Optional: log @msg
        THROW;
    END CATCH;
END;
GO

-- =============================================
-- Trigger 4: Cascade delete UserLogin when Employee is deleted
-- =============================================
CREATE OR ALTER TRIGGER dbo.trg_Employee_CascadeDeleteUserLogin
ON dbo.Employee
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DELETE ul
        FROM dbo.UserLogin ul
        INNER JOIN deleted d ON ul.ReferenceID = d.Empid
        WHERE ul.Role = 'EMPLOYEE';
    END TRY
    BEGIN CATCH
        DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
        -- Optional: log @msg
        THROW;
    END CATCH;
END;
GO

-- =============================================
-- Trigger 5: Cascade delete UserLogin when Manager is deleted
-- =============================================
CREATE OR ALTER TRIGGER dbo.trg_Manager_CascadeDeleteUserLogin
ON dbo.Manager
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DELETE ul
        FROM dbo.UserLogin ul
        INNER JOIN deleted d ON ul.ReferenceID = d.ManagerID
        WHERE ul.Role = 'MANAGER';
    END TRY
    BEGIN CATCH
        DECLARE @msg NVARCHAR(4000) = ERROR_MESSAGE();
        -- Optional: log @msg
        THROW;
    END CATCH;
END;
GO

-- =============================================
-- TEST TRIGGERS
-- =============================================

-- Test 1: Insert Customer and UserLogin
INSERT INTO Customer (Custid, Custname, DOB, Pan, Address, PhoneNumber)
VALUES ('MLA99999', 'Test User', '1990-01-01', 'TEST1234', 'Test Address', '9999999999');

INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U999', 'testuser', 'testpass', 'CUSTOMER', 'MLA99999');

-- Verify ReferenceID is set
SELECT * FROM UserLogin WHERE UserID = 'U999';

-- Test 2: Delete Customer and verify cascade delete
DELETE FROM Customer WHERE Custid = 'MLA99999';

-- Should return no rows (cascade deleted)
SELECT * FROM UserLogin WHERE UserID = 'U999';

PRINT 'All triggers created successfully!';
GO

Select * from Customer


USE Banking_Details;
GO

-- Test: Try inserting an Account opened by Manager
-- This should work now (no FK error)
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('SB99999', 'SAVING', 'MLA00001', 'U001', 'MANAGER', GETDATE(), 'OPEN');

-- Check if it was inserted
SELECT * FROM Account WHERE AccountID = 'SB99999';

-- Clean up test data
DELETE FROM Account WHERE AccountID = 'SB99999';

PRINT '✅ Database is ready! Manager can now open accounts.';
GO

USE Banking_Details;
GO

-- Step 1: Create a test customer
INSERT INTO Customer (Custid, Custname, DOB, Pan, Address, PhoneNumber)
VALUES ('MLA00002', 'Test Customer', '1990-01-01', 'TEST1234', 'Test Address', '1234567890');

-- Step 2: Now test the Account insert with Manager
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('SB99999', 'SAVING', 'MLA00002', 'U001', 'MANAGER', GETDATE(), 'OPEN');

-- Step 3: Check if it worked
SELECT * FROM Account WHERE AccountID = 'SB99999';

-- Step 4: Clean up
DELETE FROM Account WHERE AccountID = 'SB99999';
DELETE FROM Customer WHERE Custid = 'MLA00001';

PRINT '✅ Test completed! Manager can open accounts.';
GO

-- Check if Account was created
SELECT * FROM Account WHERE AccountID LIKE 'SB%';

-- Check if SavingsAccount was created
SELECT * FROM SavingsAccount WHERE SBAccountID LIKE 'SB%';

-- Check for orphaned Account entries (Account exists but SavingsAccount doesn't)
SELECT a.* 
FROM Account a
LEFT JOIN SavingsAccount sa ON a.AccountID = sa.SBAccountID
WHERE a.AccountType = 'SAVING' AND sa.SBAccountID IS NULL;


-- Find the FK constraint name
SELECT 
    fk.name AS FK_Name,
    tp.name AS Parent_Table,
    cp.name AS Parent_Column,
    tr.name AS Referenced_Table,
    cr.name AS Referenced_Column
FROM 
    sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.tables AS tp ON fkc.parent_object_id = tp.object_id
    INNER JOIN sys.columns AS cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
    INNER JOIN sys.tables AS tr ON fkc.referenced_object_id = tr.object_id
    INNER JOIN sys.columns AS cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
WHERE 
    tp.name = 'Account' AND cp.name = 'OpenedBy';


	-- Drop the FK constraint
ALTER TABLE Account
DROP CONSTRAINT FK__Account__OpenedB__2BFE89A6;  -- Replace with actual constraint name from above query

-- Add OpenedByRole column to track who opened the account
ALTER TABLE Account
ADD OpenedByRole VARCHAR(50);

-- Check FK constraints on SavingsAccount table
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM 
    sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fc ON fk.OBJECT_ID = fc.constraint_object_id
WHERE 
    OBJECT_NAME(fk.parent_object_id) = 'SavingsAccount';

    -- Check if Account SB00001 exists
SELECT * FROM Account WHERE AccountID = 'SB00001';

-- Check if Customer MLA00001 exists  
SELECT * FROM Customer WHERE Custid = 'MLA00001';

-- Now try the manual insert
INSERT INTO SavingsAccount (SBAccountID, Customerid, Balance)
VALUES ('SB00001', 'MLA00001', 5000);

SELECT * FROM SavingsAccount;
-- CORRECT ORDER: Delete child first, then parent

-- 1. Delete from SavingsAccount first
DELETE FROM SavingsAccount WHERE SBAccountID = 'SB00001';

-- 2. Then delete from Account
DELETE FROM Account WHERE AccountID = 'SB00001';

-- 3. Verify both are gone
SELECT * FROM Account WHERE AccountID = 'SB00001';
SELECT * FROM SavingsAccount WHERE SBAccountID = 'SB00001';

-- Check what's in both tables now
SELECT * FROM Account WHERE AccountID LIKE 'SB%';
SELECT * FROM SavingsAccount WHERE SBAccountID LIKE 'SB%';

-- Check what got created
SELECT 'Account' AS TableName, AccountID AS ID, CustomerID, Status FROM Account WHERE AccountID LIKE 'SB%'
UNION ALL
SELECT 'SavingsAccount' AS TableName, SBAccountID AS ID, Customerid AS CustomerID, CAST(Balance AS VARCHAR) AS Status FROM SavingsAccount WHERE SBAccountID LIKE 'SB%';

SELECT 
    UserID,
    UserName,
    Role,
    ReferenceID
FROM UserLogin
WHERE Role = 'MANAGER'

Delete from UserLogin where UserID='U001'
Select * from Manager

Select * from UserLogin


-- Insert a new Manager
INSERT INTO Manager (ManagerID, ManagerName, PAN)
VALUES ('MGR001', 'Admin Manager', 'ADMN1234');

-- Verify it was created
SELECT * FROM Manager;


-- Insert UserLogin with the Manager's ID as ReferenceID
INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U001', 'admin', 'admin123', 'MANAGER', 'MGR001');

-- Verify it was created
SELECT * FROM UserLogin WHERE Role = 'MANAGER';

-- Check that Manager and UserLogin are properly linked
SELECT 
    ul.UserID,
    ul.UserName,
    ul.Role,
    ul.ReferenceID,
    m.ManagerID,
    m.ManagerName,
    m.PAN
FROM UserLogin ul
LEFT JOIN Manager m ON ul.ReferenceID = m.ManagerID
WHERE ul.Role = 'MANAGER';

Select * from Employee

Delete From Employee where Empid='2600001'


SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Account' 
ORDER BY ORDINAL_POSITION;

-- Try to manually insert into Account table to see what error we get
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('TEST001', 'SAVING', 'MLA00002', 'MGR001', 'MANAGER', GETDATE(), 'OPEN');

-- If it fails, you'll see the exact SQL error

-- Check for triggers on Account table
SELECT 
    name AS TriggerName,
    OBJECT_NAME(parent_id) AS TableName,
    type_desc AS TriggerType
FROM sys.triggers
WHERE parent_id = OBJECT_ID('Account');

DELETE FROM Account WHERE AccountID = 'TEST001';

Select * from Account

Delete From Account 

SELECT Custid, Custname FROM Customer WHERE Custid = 'MLA00002';

Select * from UserLogin

Delete from Account where AccountID='SB00002'

-- Quick fix for empty table
DROP TABLE SavingsTransaction;

-- Create SavingsTransaction table with IDENTITY (auto-increment)

-- Verify the table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID('SavingsTransaction'), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsTransaction'
ORDER BY ORDINAL_POSITION;


-- Update all existing "Dummy" passwords to their hashed version
UPDATE UserLogin
SET PasswordHash = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg='
WHERE PasswordHash = 'Dummy';

-- Verify the update
SELECT UserID, UserName, PasswordHash, Role, ReferenceID
FROM UserLogin;

Select * from Manager
Select * from UserLogin

Select * from SavingsAccount

USE Banking_Details;
GO

ALTER TABLE SavingsTransaction
ALTER COLUMN Transactiontype VARCHAR(50) NOT NULL;
GO
