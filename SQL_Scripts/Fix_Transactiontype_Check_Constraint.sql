-- ========================================
-- Fix Check Constraint on Transactiontype
-- ========================================
-- Problem: Check constraint only allows 'WITHDRAW' and 'DEPOSIT'
-- Missing: WITHDRAWAL, INITIAL DEPOSIT, TRANSFER_DEBIT, TRANSFER_CREDIT, LOAN_PAYMENT
-- Solution: Drop old constraint and create new one with all types
-- ========================================

USE Banking_Details;
GO

PRINT '=== STEP 1: Show Current Constraint ==='
PRINT ''

SELECT 
    CONSTRAINT_NAME as 'Current Constraint',
    CHECK_CLAUSE as 'Current Allowed Values'
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS cc
INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu 
    ON cc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
WHERE cu.TABLE_NAME = 'SavingsTransaction'
AND cu.COLUMN_NAME = 'Transactiontype';

PRINT ''
PRINT 'Current constraint only allows: WITHDRAW, DEPOSIT'
PRINT 'Missing: WITHDRAWAL, INITIAL DEPOSIT, TRANSFER_DEBIT, TRANSFER_CREDIT, LOAN_PAYMENT'

PRINT ''
PRINT '=== STEP 2: Drop Old Constraint ==='
PRINT ''

-- Drop the old constraint
ALTER TABLE SavingsTransaction
DROP CONSTRAINT CK__SavingsTr__Trans__73852659;

PRINT '? Old constraint dropped!'

PRINT ''
PRINT '=== STEP 3: Create New Constraint ==='
PRINT ''

-- Create new constraint with ALL transaction types
-- Including both 'WITHDRAW' (old) and 'WITHDRAWAL' (new) for compatibility
ALTER TABLE SavingsTransaction
ADD CONSTRAINT CK_SavingsTransaction_Transactiontype
CHECK (
    Transactiontype IN (
        'DEPOSIT',              -- Original (keep for backward compatibility)
        'WITHDRAW',             -- Original (keep for backward compatibility)
        'WITHDRAWAL',           -- Used in code (same as WITHDRAW)
        'INITIAL DEPOSIT',      -- Used in account opening
        'TRANSFER_DEBIT',       -- Used in fund transfers (sender)
        'TRANSFER_CREDIT',      -- Used in fund transfers (receiver)
        'LOAN_PAYMENT'          -- Used in loan EMI payments
    )
);

PRINT '? New constraint created with ALL transaction types!'

PRINT ''
PRINT '=== STEP 4: Verify New Constraint ==='
PRINT ''

SELECT 
    CONSTRAINT_NAME as 'New Constraint Name',
    CHECK_CLAUSE as 'New Definition'
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS cc
INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu 
    ON cc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
WHERE cu.TABLE_NAME = 'SavingsTransaction'
AND cu.COLUMN_NAME = 'Transactiontype';

PRINT ''
PRINT '=== ALLOWED TRANSACTION TYPES ==='
PRINT '? DEPOSIT            (original)'
PRINT '? WITHDRAW           (original)'
PRINT '? WITHDRAWAL         (code uses this)'
PRINT '? INITIAL DEPOSIT    (account opening)'
PRINT '? TRANSFER_DEBIT     (fund transfer - sender)'
PRINT '? TRANSFER_CREDIT    (fund transfer - receiver)'
PRINT '? LOAN_PAYMENT       (loan EMI payments)'

PRINT ''
PRINT '?? Fix complete! All transaction types now allowed!'
PRINT ''
PRINT '=== NEXT STEPS ==='
PRINT '1. Restart your application'
PRINT '2. Try transferring funds'
PRINT '3. Check transaction history'
PRINT '4. Everything should work now!'

GO
