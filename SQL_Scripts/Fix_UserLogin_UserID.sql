-- ============================================
-- FIX: UserLogin Table - Separate UserID from ReferenceID
-- ============================================
-- Problem: UserID and ReferenceID have same values
-- Solution: Generate unique UserIDs (USR00001, USR00002, etc.)
-- ============================================

-- Step 1: View current data
SELECT UserID, UserName, Role, ReferenceID 
FROM UserLogin
ORDER BY Role, UserName;

-- Step 2: Backup existing UserLogin data
IF OBJECT_ID('UserLogin_Backup', 'U') IS NOT NULL
    DROP TABLE UserLogin_Backup;

SELECT * INTO UserLogin_Backup FROM UserLogin;

-- Step 3: Update UserIDs for CUSTOMER and EMPLOYEE records
-- Generate unique UserIDs while preserving ReferenceID

DECLARE @Counter INT = 1;
DECLARE @UserID VARCHAR(8);
DECLARE @CurrentUserID VARCHAR(50);

-- Create a temporary table to hold the mapping
CREATE TABLE #UserIDMapping (
    OldUserID VARCHAR(50),
    NewUserID VARCHAR(8),
    UserName VARCHAR(50),
    Role VARCHAR(20),
    ReferenceID VARCHAR(50)
);

-- Insert CUSTOMER records
INSERT INTO #UserIDMapping (OldUserID, NewUserID, UserName, Role, ReferenceID)
SELECT 
    UserID,
    'USR' + RIGHT('00000' + CAST(ROW_NUMBER() OVER (ORDER BY UserName) AS VARCHAR), 5),
    UserName,
    Role,
    ReferenceID
FROM UserLogin
WHERE Role = 'CUSTOMER';

-- Insert EMPLOYEE records
INSERT INTO #UserIDMapping (OldUserID, NewUserID, UserName, Role, ReferenceID)
SELECT 
    UserID,
    'USR' + RIGHT('00000' + CAST((SELECT COUNT(*) FROM #UserIDMapping) + ROW_NUMBER() OVER (ORDER BY UserName) AS VARCHAR), 5),
    UserName,
    Role,
    ReferenceID
FROM UserLogin
WHERE Role = 'EMPLOYEE';

-- Insert MANAGER records (keep as is, or update if needed)
INSERT INTO #UserIDMapping (OldUserID, NewUserID, UserName, Role, ReferenceID)
SELECT 
    UserID,
    UserID, -- Keep manager UserID same, or change to 'USR' format if desired
    UserName,
    Role,
    ReferenceID
FROM UserLogin
WHERE Role = 'MANAGER';

-- Step 4: Display the mapping (for review)
SELECT * FROM #UserIDMapping ORDER BY Role, NewUserID;

-- Step 5: Update UserLogin table with new UserIDs
UPDATE ul
SET ul.UserID = m.NewUserID
FROM UserLogin ul
INNER JOIN #UserIDMapping m ON ul.UserID = m.OldUserID;

-- Step 6: Verify the changes
SELECT UserID, UserName, Role, ReferenceID 
FROM UserLogin
ORDER BY Role, UserID;

-- Step 7: Verify UserID is now unique
SELECT UserID, COUNT(*) as Count
FROM UserLogin
GROUP BY UserID
HAVING COUNT(*) > 1;
-- (Should return no rows)

-- Step 8: Verify ReferenceID still points to correct records
-- For Customers
SELECT 
    ul.UserID,
    ul.UserName,
    ul.Role,
    ul.ReferenceID,
    c.Custname
FROM UserLogin ul
LEFT JOIN Customer c ON ul.ReferenceID = c.Custid
WHERE ul.Role = 'CUSTOMER';

-- For Employees
SELECT 
    ul.UserID,
    ul.UserName,
    ul.Role,
    ul.ReferenceID,
    e.EmployeeName
FROM UserLogin ul
LEFT JOIN Employee e ON ul.ReferenceID = e.Empid
WHERE ul.Role = 'EMPLOYEE';

-- Step 9: Clean up
DROP TABLE #UserIDMapping;

-- Optional: Drop backup table after confirming everything works
-- DROP TABLE UserLogin_Backup;

-- ============================================
-- EXPECTED RESULT:
-- ============================================
-- UserID      | UserName   | Role      | ReferenceID
-- ------------|------------|-----------|-------------
-- USR00001    | john1234   | CUSTOMER  | MLA00001
-- USR00002    | jane5678   | CUSTOMER  | MLA00002
-- USR00003    | bob9012    | EMPLOYEE  | 26000001
-- MGR001      | admin      | MANAGER   | MGR001
-- ============================================
