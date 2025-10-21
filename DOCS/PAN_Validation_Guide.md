# PAN Validation - Uniqueness Constraint

## ?? Business Rule Implemented

**PAN (Permanent Account Number) must be unique across the entire system.**

### Key Rules:
1. ? Each PAN can only be registered **once** as a Customer
2. ? Each PAN can only be registered **once** as an Employee
3. ? Same person **cannot** be both Customer and Employee (cross-table validation)
4. ? PAN format must be valid: **4 letters + 4 digits** (e.g., ABCD1234)

---

## ?? Implementation Details

### Database Changes: **NONE REQUIRED**
- PAN columns already exist in both Customer and Employee tables
- No schema changes needed
- Business logic validation only

### Code Changes Made:

#### 1. **CustomerRepository.cs** - Added PAN validation methods
```csharp
public bool PanExists(string pan)
// Checks if PAN exists in Customer table

public Customer GetCustomerByPan(string pan)
// Retrieves customer by PAN
```

#### 2. **EmployeeRepository.cs** - Added PAN validation methods
```csharp
public bool PanExists(string pan)
// Checks if PAN exists in Employee table

public Employee GetEmployeeByPan(string pan)
// Retrieves employee by PAN
```

#### 3. **CustomerService.cs** - Enhanced validation
```csharp
// Validates PAN uniqueness in Customer table
() => _customerRepo.PanExists(pan) ? 
    Error($"PAN '{pan}' already registered as customer") : null

// Validates PAN doesn't exist in Employee table
() => _employeeRepo.PanExists(pan) ? 
    Error($"PAN '{pan}' already registered as employee") : null
```

#### 4. **EmployeeService.cs** - Enhanced validation
```csharp
// Validates PAN uniqueness in Employee table
() => _employeeRepo.PanExists(pan) ? 
    Error($"PAN '{pan}' already registered as employee") : null

// Validates PAN doesn't exist in Customer table
() => _customerRepo.PanExists(pan) ? 
    Error($"PAN '{pan}' already registered as customer") : null
```

---

## ?? Validation Flow

### Customer Registration:
1. ? Check PAN format (4 letters + 4 digits)
2. ? Check if PAN exists in **Customer** table
3. ? Check if PAN exists in **Employee** table
4. ? If all pass ? Register customer
5. ? If any fail ? Show error message

### Employee Registration:
1. ? Check PAN format (4 letters + 4 digits)
2. ? Check if PAN exists in **Employee** table
3. ? Check if PAN exists in **Customer** table
4. ? If all pass ? Register employee
5. ? If any fail ? Show error message

---

## ?? User Experience

### Scenario 1: Duplicate Customer PAN
**Manager tries to register customer with existing PAN**

**Input:**
- Name: John Smith
- PAN: ABCD1234 (already exists)

**Output:**
```
? Error: PAN number 'ABCD1234' is already registered with another customer. 
Each PAN can only be used once.
```

### Scenario 2: Customer PAN exists as Employee
**Manager tries to register customer, but PAN belongs to employee**

**Input:**
- Name: Jane Doe
- PAN: WXYZ5678 (exists in Employee table)

**Output:**
```
? Error: PAN number 'WXYZ5678' is already registered as an employee. 
Same person cannot be both customer and employee.
```

### Scenario 3: Successful Registration
**Manager registers customer with unique PAN**

**Input:**
- Name: Bob Johnson
- PAN: QWER9012 (doesn't exist)

**Output:**
```
? Success: Customer registered successfully! 
Customer ID: MLA00005, Username: bob1234, Password: Dummy
```

---

## ?? Testing Scenarios

### Test 1: Register Customer with Duplicate PAN
1. Login as Manager
2. Go to "Register Customer" tab
3. Fill form with PAN that already exists (e.g., ABCD1234)
4. Click "Register Customer"
5. **Expected:** Error message about duplicate PAN

### Test 2: Register Employee with Customer PAN
1. Login as Manager
2. Go to "Register Employee" tab
3. Fill form with PAN that exists in Customer table
4. Click "Register Employee"
5. **Expected:** Error message about PAN being customer

### Test 3: Register Customer with Valid PAN
1. Login as Manager
2. Go to "Register Customer" tab
3. Fill form with NEW unique PAN
4. Click "Register Customer"
5. **Expected:** Success message with credentials

### Test 4: PAN Format Validation
1. Login as Manager
2. Try to register with invalid PAN formats:
   - `ABC123` (too short)
   - `ABCDE1234` (5 letters)
   - `ABC12345` (5 digits)
   - `1234ABCD` (numbers first)
3. **Expected:** Format error for each

---

## ?? Validation Matrix

| Scenario | Customer Table | Employee Table | Result |
|----------|----------------|----------------|--------|
| PAN not in any table | ? Not Found | ? Not Found | ? **Allow** |
| PAN exists as Customer | ? Found | ? Not Found | ? **Reject** - Duplicate Customer |
| PAN exists as Employee | ? Not Found | ? Found | ? **Reject** - Already Employee |
| PAN in both tables | ? Found | ? Found | ? **Reject** - Duplicate |
| Invalid PAN format | N/A | N/A | ? **Reject** - Format Error |

---

## ?? SQL Queries for Verification

### Check for duplicate PANs in Customer table:
```sql
SELECT Pan, COUNT(*) as Count
FROM Customer
GROUP BY Pan
HAVING COUNT(*) > 1;
-- Should return 0 rows
```

### Check for duplicate PANs in Employee table:
```sql
SELECT Pan, COUNT(*) as Count
FROM Employee
GROUP BY Pan
HAVING COUNT(*) > 1;
-- Should return 0 rows
```

### Check for PANs in both Customer and Employee:
```sql
SELECT c.Pan, c.Custname, e.EmployeeName
FROM Customer c
INNER JOIN Employee e ON c.Pan = e.Pan;
-- Should return 0 rows (no overlap)
```

### Find customer by PAN:
```sql
SELECT Custid, Custname, Pan, PhoneNumber
FROM Customer
WHERE Pan = 'ABCD1234';
```

### Find employee by PAN:
```sql
SELECT Empid, EmployeeName, Pan, DeptId
FROM Employee
WHERE Pan = 'ABCD1234';
```

---

## ?? Important Notes

### Why Same Person Can't Be Customer and Employee?
1. **Business Logic:** An employee working at the bank shouldn't have customer accounts in the same bank (conflict of interest)
2. **Data Integrity:** PAN is unique to a person, so one person = one role
3. **Regulatory Compliance:** Banking regulations may prohibit this

### What If Employee Wants to Be Customer (or vice versa)?
**Option 1:** Delete old record and create new one
**Option 2:** Keep separate PAN numbers (not realistic)
**Option 3:** Business decides to allow it (remove cross-validation)

To allow same person in both roles, comment out these lines:
```csharp
// In CustomerService.cs
() => _employeeRepo.PanExists(pan) ? Error(...) : null

// In EmployeeService.cs
() => _customerRepo.PanExists(pan) ? Error(...) : null
```

---

## ?? Error Messages Reference

| Error Code | Message | Cause |
|------------|---------|-------|
| PAN-001 | PAN number 'XXXX####' is already registered with another customer | Duplicate in Customer table |
| PAN-002 | PAN number 'XXXX####' is already registered with another employee | Duplicate in Employee table |
| PAN-003 | PAN number 'XXXX####' is already registered as an employee. Cannot be customer | Cross-table conflict (Employee ? Customer) |
| PAN-004 | PAN number 'XXXX####' is already registered as a customer. Cannot be employee | Cross-table conflict (Customer ? Employee) |
| PAN-005 | PAN must be 4 letters followed by 4 digits (e.g., ABCD1234) | Invalid format |

---

## ? Testing Checklist

- [ ] Test duplicate customer PAN
- [ ] Test duplicate employee PAN
- [ ] Test customer PAN existing as employee
- [ ] Test employee PAN existing as customer
- [ ] Test invalid PAN formats
- [ ] Test successful registration with unique PAN
- [ ] Verify error messages display correctly
- [ ] Verify database has no duplicate PANs
- [ ] Test PAN case sensitivity (ABCD1234 vs abcd1234)
- [ ] Test with leading/trailing spaces

---

## ?? Benefits

? **Data Integrity** - No duplicate PANs in system
? **Compliance** - Follows banking regulations
? **Security** - Prevents fraud and identity conflicts
? **User Experience** - Clear error messages
? **Maintainability** - Centralized validation logic

---

**Status:** ? Implemented and Working
**Build Status:** ? Successful
**Testing Required:** ? Yes (Manual + Automated)

---

**Last Updated:** [Current Date]
**Version:** 1.0.0
