-- ============================================================
-- CREATE FUNDTRANSFER TABLE FOR MONEY TRANSFERS
-- ============================================================
-- Tracks all fund transfers between customer savings accounts
-- ============================================================

USE Banking_Details;
GO

PRINT '========================================';
PRINT 'CREATING FUNDTRANSFER TABLE';
PRINT '========================================';
PRINT '';

-- Drop if exists
IF OBJECT_ID('FundTransfer', 'U') IS NOT NULL
BEGIN
    DROP TABLE FundTransfer;
    PRINT 'Existing FundTransfer table dropped';
END
PRINT '';

-- Create FundTransfer table
CREATE TABLE FundTransfer (
    TransferID INT IDENTITY(1,1) PRIMARY KEY,
    FromAccountID CHAR(7) NOT NULL,
    ToAccountID CHAR(7) NOT NULL,
    Amount DECIMAL(12,2) NOT NULL CHECK (Amount > 0),
    TransferDate DATETIME NOT NULL DEFAULT GETDATE(),
    FromCustomerID CHAR(8) NOT NULL,
    ToCustomerID CHAR(8) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'SUCCESS', -- SUCCESS, FAILED, PENDING
    Remarks VARCHAR(200),
    CONSTRAINT FK_FundTransfer_FromAccount FOREIGN KEY (FromAccountID) REFERENCES SavingsAccount(SBAccountID),
    CONSTRAINT FK_FundTransfer_ToAccount FOREIGN KEY (ToAccountID) REFERENCES SavingsAccount(SBAccountID),
    CONSTRAINT FK_FundTransfer_FromCustomer FOREIGN KEY (FromCustomerID) REFERENCES Customer(Custid),
    CONSTRAINT FK_FundTransfer_ToCustomer FOREIGN KEY (ToCustomerID) REFERENCES Customer(Custid),
    CONSTRAINT CK_FundTransfer_DifferentAccounts CHECK (FromAccountID != ToAccountID),
    CONSTRAINT CK_FundTransfer_MinAmount CHECK (Amount >= 100),
    CONSTRAINT CK_FundTransfer_MaxAmount CHECK (Amount <= 100000)
);

PRINT '? FundTransfer table created';
PRINT '';

-- Create indexes for performance
CREATE INDEX IX_FundTransfer_FromAccount ON FundTransfer(FromAccountID);
CREATE INDEX IX_FundTransfer_ToAccount ON FundTransfer(ToAccountID);
CREATE INDEX IX_FundTransfer_Date ON FundTransfer(TransferDate);

PRINT '? Indexes created';
PRINT '';

-- Verify structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'FundTransfer'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '========================================';
PRINT 'FUNDTRANSFER TABLE CREATED!';
PRINT '========================================';
PRINT '';
PRINT 'Table Structure:';
PRINT '? TransferID       INT IDENTITY    - PRIMARY KEY';
PRINT '? FromAccountID    CHAR(7)         - Sender savings account';
PRINT '? ToAccountID      CHAR(7)         - Receiver savings account';
PRINT '? Amount           DECIMAL(12,2)   - Transfer amount';
PRINT '? TransferDate     DATETIME        - Auto-timestamp';
PRINT '? FromCustomerID   CHAR(8)         - Sender customer';
PRINT '? ToCustomerID     CHAR(8)         - Receiver customer';
PRINT '? Status           VARCHAR(20)     - SUCCESS/FAILED/PENDING';
PRINT '? Remarks          VARCHAR(200)    - Optional notes';
PRINT '';
PRINT 'Constraints:';
PRINT '? Min amount: Rs. 100';
PRINT '? Max amount: Rs. 1,00,000 per transaction';
PRINT '? Cannot transfer to same account';
PRINT '? FK to SavingsAccount (From & To)';
PRINT '? FK to Customer (From & To)';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Update Entity Framework model';
PRINT '2. Create FundTransferService';
PRINT '3. Add transfer functionality to Customer Dashboard';
PRINT '';

GO
