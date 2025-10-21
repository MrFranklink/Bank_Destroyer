SELECT Custid, Custname FROM Customer WHERE Custid = 'MLA00002';

SELECT AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, Status 
FROM Account 
WHERE CustomerID = 'MLA00002';

SELECT SBAccountID, Customerid, Balance 
FROM SavingsAccount 
WHERE Customerid = 'MLA00002';


SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Account'
ORDER BY ORDINAL_POSITION;


SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('Account');


-- 1. Check Account table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Account'
ORDER BY ORDINAL_POSITION;

-- 2. Check all Foreign Keys on Account table
SELECT 
    fk.name AS FK_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column,
    delete_referential_action_desc,
    update_referential_action_desc
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID('Account');

-- 3. Check if there are any CHECK constraints
SELECT 
    con.name AS Constraint_Name,
    col.name AS Column_Name
FROM sys.check_constraints con
INNER JOIN sys.columns col ON con.parent_object_id = col.object_id
WHERE con.parent_object_id = OBJECT_ID('Account');

-- 4. Verify Customer exists
SELECT Custid, Custname FROM Customer WHERE Custid = 'MLA00002';

-- 5. Check if any Account exists for this customer
SELECT * FROM Account WHERE CustomerID = 'MLA00002';


INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('TEST999', 'SAVING', 'MLA00002', 'MGR001', 'MANAGER', GETDATE(), 'OPEN');

SELECT * FROM Account WHERE AccountID = 'TEST999';

-- Test 1: Try with NULL OpenedBy
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('TEST111', 'SAVING', 'MLA00002', NULL, NULL, GETDATE(), 'OPEN');

-- Test 2: Try with empty string OpenedBy
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('TEST222', 'SAVING', 'MLA00002', '', '', GETDATE(), 'OPEN');

-- Test 3: Try the exact values from your web app
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('SB00001', 'SAVING', 'MLA00002', 'MGR001', 'MANAGER', GETDATE(), 'OPEN');

SELECT 
    con.name AS Constraint_Name,
    con.definition AS Constraint_Definition
FROM sys.check_constraints con
WHERE con.parent_object_id = OBJECT_ID('Account');

-- Test with different OpenedBy values
INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('TEST333', 'SAVING', 'MLA00002', 'M', 'MANAGER', GETDATE(), 'OPEN');

INSERT INTO Account (AccountID, AccountType, CustomerID, OpenedBy, OpenedByRole, OpenDate, Status)
VALUES ('TEST444', 'SAVING', 'MLA00002', 'MGR', 'MANAGER', GETDATE(), 'OPEN');

-- See what Savings Account IDs already exist
SELECT SBAccountID FROM SavingsAccount ORDER BY SBAccountID DESC;

-- See what Account IDs exist for SAVING type
SELECT AccountID FROM Account WHERE AccountType = 'SAVING' ORDER BY AccountID DESC;

SELECT * FROM Account WHERE CustomerID = 'MLA00005';
SELECT * FROM SavingsAccount WHERE Customerid = 'MLA00005';

DELETE FROM Account WHERE CustomerID = 'MLA00005';

Select * from FDTransaction
Select * from FixedDepositAccount