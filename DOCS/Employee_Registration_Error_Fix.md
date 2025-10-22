# ?? Employee Registration Error Fix Guide

## ? **Error:** "Failed to register employee. Please try again."

---

## ?? **What I Changed:**

### 1?? **EmployeeRepository.cs**
- ? Changed `catch` block to expose actual exception
- ? Added detailed error logging
- ? Now throws exception instead of silently returning false

### 2?? **EmployeeService.cs**
- ? Updated to catch and display detailed error messages
- ? Shows inner exception details
- ? Better error messages for debugging

---

## ?? **Next Steps to Fix:**

### **Step 1: Run Diagnostic Script**

Open **SQL Server Management Studio** and run:
```
SQL_Scripts/Diagnose_Employee_Registration.sql
```

This will check:
- ? Employee table structure
- ? Pan column exists
- ? Foreign key constraints
- ? Department table
- ? Duplicate PANs
- ? Test insert

**Look for any ? errors in the output!**

---

### **Step 2: Try Registering Employee Again**

1. **Run your application** (F5)
2. **Login as Manager:**
   - Username: `admin`
   - Password: `Dummy`
3. **Go to "Register Employee" tab**
4. **Fill in:**
   ```
   Employee Name: Test Employee
   Department: DEPT01
   PAN: FGHIJ5678K
   ```
5. **Click "Register Employee"**

---

### **Step 3: Check Error Message**

The error message will now show **EXACTLY** what's wrong!

**Examples:**

? **"Foreign key constraint error"**
? Department table missing or invalid DeptId

? **"Duplicate key violation"**
? PAN already exists in database

? **"Cannot insert NULL"**
? Required field is missing

? **"Invalid column name 'Pan'"**
? Pan column doesn't exist in database

---

## ?? **Common Fixes:**

### **Fix 1: Pan Column Missing**
```sql
ALTER TABLE Employee ADD Pan VARCHAR(10) NULL;
```

### **Fix 2: Department Table Missing**
```sql
-- Check if Department table exists
SELECT * FROM Department;

-- If missing, create it:
CREATE TABLE Department (
    DeptID VARCHAR(10) PRIMARY KEY,
    DeptName VARCHAR(50) NOT NULL
);

-- Insert departments
INSERT INTO Department VALUES ('DEPT01', 'Deposit Management');
INSERT INTO Department VALUES ('DEPT02', 'Loan Management');
INSERT INTO Department VALUES ('DEPT03', 'HR Department');
```

### **Fix 3: Foreign Key Constraint Error**
```sql
-- Check if DeptId foreign key exists
SELECT name FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Employee');

-- If yes, drop it temporarily:
ALTER TABLE Employee DROP CONSTRAINT FK_Employee_Department_DeptId;

-- Or ensure Department table has DEPT01, DEPT02, DEPT03
```

### **Fix 4: Duplicate PAN**
```sql
-- Check for duplicate PANs
SELECT Pan, COUNT(*) FROM Employee GROUP BY Pan HAVING COUNT(*) > 1;

-- Delete duplicates if needed
DELETE FROM Employee WHERE Empid = '2600001' AND Pan = 'FGHIJ5678K';
```

---

## ?? **Debugging Checklist:**

### **Before Running Application:**
- [ ] SQL diagnostic script ran successfully
- [ ] Pan column exists (VARCHAR(10))
- [ ] Department table has DEPT01, DEPT02, DEPT03
- [ ] No duplicate PANs in database
- [ ] Application built successfully

### **After Clicking Register Employee:**
- [ ] Check error message on screen (now shows details!)
- [ ] Check Visual Studio **Output** window (Debug output)
- [ ] Check SQL Server **error logs**

---

## ?? **Expected Behavior:**

### **? Success:**
```
Success! Employee registered successfully! 
Employee ID: 2600001
Username: test1234
Password: Dummy
Department: DEPT01

Employee Login Credentials
ID: 2600001
Username: test1234
Password: Dummy
Please save these credentials securely!
```

### **? Failure (with details):**
```
Error! Registration failed: Cannot insert duplicate key in object 'dbo.Employee'. 
The duplicate key value is (FGHIJ5678K). | Details: The statement has been terminated.
```

---

## ?? **Pro Tips:**

1. **Always check Output window** in Visual Studio (View ? Output)
2. **Run diagnostic script** before testing
3. **Use unique PAN** for each test (e.g., TEST12345A, TEST67890B)
4. **Check Department table** has valid DeptIDs
5. **Rebuild solution** after code changes (Ctrl+Shift+B)

---

## ?? **If Error Still Occurs:**

### **Option 1: Show Me the Error**
1. Register employee
2. Copy the **EXACT error message**
3. Send it to me
4. I'll tell you exactly how to fix it

### **Option 2: Check SQL Server**
```sql
-- Check Employee table
SELECT TOP 5 * FROM Employee ORDER BY Empid DESC;

-- Check UserLogin for employees
SELECT * FROM UserLogin WHERE Role = 'EMPLOYEE';

-- Check Department table
SELECT * FROM Department;
```

### **Option 3: Fresh Start**
```sql
-- Delete all employees (WARNING: This deletes all data!)
DELETE FROM UserLogin WHERE Role = 'EMPLOYEE';
DELETE FROM Employee;

-- Reset auto-increment (if needed)
-- Next employee will be 2600001
```

---

## ?? **Files Modified:**

1. ? `DB/EmployeeRepository.cs` - Better error handling
2. ? `BankApp.Services/EmployeeService.cs` - Detailed error messages
3. ? `SQL_Scripts/Diagnose_Employee_Registration.sql` - Diagnostic tool

---

## ? **Next Steps:**

1. **Run diagnostic SQL script**
2. **Try registering employee**
3. **Look at error message** (now shows details!)
4. **Send me the error** if you need help

The error message will now tell us **exactly** what's wrong! ??

---

## ?? **Once It Works:**

You should see:
- ? Success message
- ? Credentials box (Employee ID, Username, Password)
- ? New employee in "View Lists" ? "Employees" tab
- ? Can login with generated username/password

**Let me know what error message you get now!** ??
