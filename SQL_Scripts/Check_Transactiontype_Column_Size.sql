-- Check current column size for Transactiontype
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SavingsTransaction' 
AND COLUMN_NAME = 'Transactiontype';

-- This will show you the current size
-- Likely shows: VARCHAR(10)
