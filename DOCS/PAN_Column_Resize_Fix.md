# ?? PAN Column Resize Error Fix

## ? **The Error:**
```
Msg 5074, Level 16, State 1, Line 69
The object 'UQ__Manager__C577943D7D8D0083' is dependent on column 'Pan'.
```

## ?? **Root Cause:**
SQL Server cannot ALTER a column size when a **UNIQUE constraint** exists on that column.

**Affected Tables:**
- ? Customer.Pan - Updated successfully
- ? Employee.Pan - Updated successfully  
- ? Manager.Pan - **BLOCKED by UNIQUE constraint**

---

## ? **Solution Steps:**

### **Step 1: Drop UNIQUE Constraints**
```sql
ALTER TABLE Manager DROP CONSTRAINT UQ__Manager__C577943D7D8D0083;
```

### **Step 2: Alter Column Size**
```sql
ALTER TABLE Manager ALTER COLUMN Pan VARCHAR(10) NULL;
```

### **Step 3: Recreate UNIQUE Constraint**
```sql
ALTER TABLE Manager ADD CONSTRAINT UQ_Manager_Pan UNIQUE (Pan);
```

---

## ?? **Automated Solution:**

The **updated SQL script** (`SQL_Scripts/Update_PAN_Format_To_Real.sql`) now:

1. ? **Automatically finds** all UNIQUE constraints on Pan columns
2. ? **Drops them** before altering columns
3. ? **Alters column size** from VARCHAR(8) to VARCHAR(10)
4. ? **Recreates constraints** with standard naming:
   - `UQ_Customer_Pan`
   - `UQ_Employee_Pan`
   - `UQ_Manager_Pan`

---

## ?? **How to Run Fixed Script:**

### **Option 1: Run Full Script (Recommended)**
```sql
-- In SSMS, open: SQL_Scripts/Update_PAN_Format_To_Real.sql
-- Press F5 to execute
```

### **Option 2: Manual Fix (If needed)**
```sql
-- 1. Find constraint name
SELECT name 
FROM sys.key_constraints 
WHERE parent_object_id = OBJECT_ID('Manager') 
  AND type = 'UQ';

-- 2. Drop constraint (replace with actual name)
ALTER TABLE Manager DROP CONSTRAINT UQ__Manager__C577943D7D8D0083;

-- 3. Alter column
ALTER TABLE Manager ALTER COLUMN Pan VARCHAR(10) NULL;

-- 4. Recreate constraint
ALTER TABLE Manager ADD CONSTRAINT UQ_Manager_Pan UNIQUE (Pan);
```

---

## ? **Verification:**

After running the script, verify:

### **1. Column Size Updated:**
```sql
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'Pan';
```

**Expected Output:**
```
Customer    Pan    10
Employee    Pan    10
Manager     Pan    10
```

### **2. UNIQUE Constraints Recreated:**
```sql
SELECT 
    OBJECT_NAME(parent_object_id) AS TableName,
    name AS ConstraintName
FROM sys.key_constraints
WHERE parent_object_id IN (
    OBJECT_ID('Customer'), 
    OBJECT_ID('Employee'), 
    OBJECT_ID('Manager')
)
AND type = 'UQ';
```

**Expected Output:**
```
Customer    UQ_Customer_Pan
Employee    UQ_Employee_Pan
Manager     UQ_Manager_Pan
```

---

## ?? **What the Fixed Script Does:**

```sql
-- 1. Backup data
SELECT * INTO Customer_PAN_Backup FROM Customer;
SELECT * INTO Employee_PAN_Backup FROM Employee;
SELECT * INTO Manager_PAN_Backup FROM Manager;

-- 2. Drop all UNIQUE constraints on Pan columns
ALTER TABLE Customer DROP CONSTRAINT [constraint_name];
ALTER TABLE Employee DROP CONSTRAINT [constraint_name];
ALTER TABLE Manager DROP CONSTRAINT [constraint_name];

-- 3. Alter column sizes
ALTER TABLE Customer ALTER COLUMN Pan VARCHAR(10) NULL;
ALTER TABLE Employee ALTER COLUMN Pan VARCHAR(10) NULL;
ALTER TABLE Manager ALTER COLUMN Pan VARCHAR(10) NULL;

-- 4. Recreate UNIQUE constraints with standard names
ALTER TABLE Customer ADD CONSTRAINT UQ_Customer_Pan UNIQUE (Pan);
ALTER TABLE Employee ADD CONSTRAINT UQ_Employee_Pan UNIQUE (Pan);
ALTER TABLE Manager ADD CONSTRAINT UQ_Manager_Pan UNIQUE (Pan);
```

---

## ?? **Key Points:**

1. ? **Always drop constraints BEFORE altering columns**
2. ? **Backup data before making schema changes**
3. ? **Use standard naming** for constraints (e.g., `UQ_TableName_ColumnName`)
4. ? **Verify changes** after running script
5. ? **Test application** to ensure PAN validation works

---

## ?? **Next Steps:**

1. ? **Run the fixed script:** `SQL_Scripts/Update_PAN_Format_To_Real.sql`
2. ? **Verify column sizes:** All should be VARCHAR(10)
3. ? **Verify constraints:** All should exist with standard names
4. ? **Test application:** Register customer with `ABCDE1234F`
5. ? **Update existing PANs:** Convert old 8-char to new 10-char format
6. ? **Drop backup tables:** Once everything is confirmed working

---

## ?? **Result:**

After running the fixed script:
- ? All PAN columns are VARCHAR(10)
- ? All UNIQUE constraints are recreated
- ? Application can accept 10-character PANs
- ? Validation enforces: `ABCDE1234F` format

---

## ?? **If You Still Get Errors:**

### **Error: Constraint still exists**
```sql
-- Manually find and drop it
EXEC sp_helpconstraint 'Manager';
ALTER TABLE Manager DROP CONSTRAINT [actual_constraint_name];
```

### **Error: Duplicate PANs exist**
```sql
-- Find duplicates
SELECT Pan, COUNT(*) 
FROM Manager 
GROUP BY Pan 
HAVING COUNT(*) > 1;

-- Fix duplicates before recreating constraint
```

---

## ?? **Related Files:**
- ? `SQL_Scripts/Update_PAN_Format_To_Real.sql` - Fixed migration script
- ? `DB/Utilities/IdGenerator.cs` - PAN validation logic
- ? `README.md` - Documentation updated

**Now re-run the SQL script - it should work!** ??
