create Database Banking_Details
 use Banking_Details

 CREATE TABLE Employee (
    Empid VARCHAR(20) PRIMARY KEY CHECK (LEFT(Empid, 2) = '26'),
    EmployeeName VARCHAR(20) NOT NULL,
    DeptId CHAR(6),
    Pan VARCHAR(8) NOT NULL CHECK (Pan NOT LIKE '%[^a-zA-Z0-9]%'),
    FOREIGN KEY (DeptId) REFERENCES Department(Deptid)
);



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
    Transactionid INT PRIMARY KEY,
    SBAccountID CHAR(7),
    Transationdate DATE DEFAULT GETDATE(),
    Transactiontype VARCHAR(10) CHECK (Transactiontype IN ('Deposit','Withdraw')),
    Amount SMALLMONEY CHECK (Amount >= 100),
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
    FOREIGN KEY (CustomerID) REFERENCES Customer(Custid)
);



CREATE TABLE FDTransaction (
    TransactionID INT PRIMARY KEY,
    FDAccountID CHAR(7),
    TransactionType VARCHAR(15) CHECK (TransactionType = 'FORECLOSE'),
    Amount SMALLMONEY NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (FDAccountID) REFERENCES FixedDepositAccount(FDAccountID)
);


INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U001', 'admin', 'admin123', 'Manager', NULL);
UPDATE UserLogin
SET Role = 'Manager'
WHERE UserID='U001';

INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U002', 'admin1', 'admin123', 'Customer', NULL);

INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('U003', 'admin2', 'admin123', 'Employee', NULL);

Select * from UserLogin
---Later work when we delete some emp it will delete for others and some trigger work we have to do



-- Check if Department table has any records
SELECT * FROM Department;

-- If empty, insert a test department
INSERT INTO Department (Deptid, Deptname)
VALUES ('DEPT01', 'Sales Department');

INSERT INTO Department (Deptid, Deptname)
VALUES ('DEPT02', 'IT Department');

INSERT INTO Department (Deptid, Deptname)
VALUES ('DEPT03', 'HR Department');