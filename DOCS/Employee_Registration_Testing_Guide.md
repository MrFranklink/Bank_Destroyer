# ? Employee Registration - Complete Validation Testing Guide

## ?? **Validation Rules Summary**

| Field | Rules | Example Valid | Example Invalid |
|-------|-------|---------------|-----------------|
| **Employee Name** | • Required<br>• Min 3, Max 50 chars<br>• Only letters, spaces, dots (.) | `Amit Kumar`<br>`A. K. Sharma` | `Amit123` ?<br>`AB` ?<br>`Amit@Kumar` ? |
| **Department** | • Required<br>• Must be DEPT01, DEPT02, or DEPT03 | `DEPT01`<br>`DEPT02` | `DEPT04` ?<br>`(empty)` ? |
| **PAN Number** | • **Required** ?<br>• Format: ABCDE1234F<br>• Unique in Employee table<br>• **Cannot exist in Customer table**<br>• Cannot be logged-in Manager's PAN (future) | `FGHIJ5678K`<br>` KLMNO9012P` | `ABCD1234` ? (8 chars)<br>`ABCDE1234F` ? (if already Customer) |

---

## ?? **Access Control:**

| Role | Can Access "Register Employee"? | Can Register Others? | Can Register Self? |
|------|----------------------------------|---------------------|-------------------|
| **Manager** | ? YES (only in Manager Dashboard) | ? YES | ? NO (future check) |
| **Employee** | ? NO (tab hidden) | ? NO | ? NO |
| **Customer** | ? NO | ? NO | ? NO |

---

## ?? **Test Cases**

### ? **Test Case 1: Valid Employee Registration (Manager)**
```
Logged in as: Manager (admin / Dummy)
Employee Name: Priya Patel
Department: DEPT01
PAN: FGHIJ5678K (new, not in Customer or Employee)
```
**Expected:** ? Success - Employee registered with credentials displayed

---

### ? **Test Case 2: Employee Tries to Access Register Employee Tab**
```
Logged in as: Employee (DEPT01 or DEPT02)
Action: Try to access "Register Employee" tab
```
**Expected:** ? Tab is hidden - not visible in Employee Dashboard

---

### ? **Test Case 3: Invalid Name (Numbers)**
```
Logged in as: Manager
Employee Name: John123
Department: DEPT01
PAN: FGHIJ5678K
```
**Expected:** ? Error - "Employee Name can only contain letters, spaces, and dots (.)"

---

### ? **Test Case 4: Invalid Name (Too Short)**
```
Logged in as: Manager
Employee Name: AB
Department: DEPT01
PAN: FGHIJ5678K
```
**Expected:** ? Error - "Employee Name must be at least 3 characters long"

---

### ? **Test Case 5: Invalid Department**
```
Logged in as: Manager
Employee Name: Test Employee
Department: DEPT04
PAN: FGHIJ5678K
```
**Expected:** ? Error - "Department ID must be DEPT01, DEPT02, or DEPT03"

---

### ? **Test Case 6: Invalid PAN Format (8 chars)**
```
Logged in as: Manager
Employee Name: Test Employee
Department: DEPT01
PAN: ABCD1234 (old 8-char format)
```
**Expected:** ? Error - "PAN must be in format: 5 letters + 4 digits + 1 letter"

---

### ? **Test Case 7: Duplicate PAN in Employee Table**
```
Step 1: Register Employee with PAN FGHIJ5678K ? Success
Step 2: Try to register another Employee with same PAN FGHIJ5678K
```
**Expected:** ? Error - "PAN number 'FGHIJ5678K' is already registered with another Employee"

---

### ? **Test Case 8: PAN Exists as Customer (Dual Role Prevention)**
```
Step 1: Register Customer with PAN ABCDE1234F ? Success
Step 2: Manager tries to register Employee with same PAN ABCDE1234F
```
**Expected:** ? Error - "PAN number 'ABCDE1234F' is already registered as a Customer. Same person cannot be both Employee and Customer."

---

### ? **Test Case 9: Customer Tries to Register Employee (Reverse Dual Role)**
```
Step 1: Manager registers Employee with PAN FGHIJ5678K ? Success
Step 2: Manager tries to register Customer with same PAN FGHIJ5678K
```
**Expected:** ? Error - "PAN number 'FGHIJ5678K' is already registered as an employee. Same person cannot be both customer and employee."

---

### ? **Test Case 10: Empty PAN**
```
Logged in as: Manager
Employee Name: Test Employee
Department: DEPT01
PAN: (empty)
```
**Expected:** ? Error - "PAN is required"

---

### ? **Test Case 11: PAN with Special Characters**
```
Logged in as: Manager
Employee Name: Test Employee
Department: DEPT01
PAN: ABCD@1234F
```
**Expected:** ? Blocked by HTML5 pattern validation

---

### ? **Test Case 12: Valid Registration with Dots in Name**
```
Logged in as: Manager
Employee Name: A. K. Sharma
Department: DEPT02
PAN: KLMNO9012P
```
**Expected:** ? Success - Name with dots is valid

---

### ? **Test Case 13: Self-Registration Attempt (Future)**
```
Logged in as: Manager (PAN: MGPAN1234M)
Action: Try to register Employee with PAN: MGPAN1234M (same as logged-in Manager)
```
**Expected:** ? Error - "You cannot register yourself as an Employee. Please contact your Manager."
**Note:** Currently not fully implemented since Manager table doesn't have PAN column

---

## ?? **Frontend Validation (HTML5)**

### **Manager Dashboard - Register Employee Form:**
```html
<input name="empName" 
       minlength="3" 
       maxlength="50" 
       pattern="[a-zA-Z\s.]+" 
       title="Only letters, spaces, and dots allowed"
       required />

<select name="deptId" required>
  <option value="">-- Select Department --</option>
  <option value="DEPT01">DEPT01 - Deposit Management</option>
  <option value="DEPT02">DEPT02 - Loan Management</option>
  <option value="DEPT03">DEPT03 - HR Department</option>
</select>

<input name="pan" 
       maxlength="10" 
       pattern="[A-Z]{5}[0-9]{4}[A-Z]" 
       style="text-transform: uppercase;" 
       title="Format: 5 letters + 4 digits + 1 letter"
       required />
```

### **Employee Dashboard:**
- ? **"Register Employee" tab is HIDDEN**
- ? Only shows: Register Customer, Open Accounts, Manage Accounts, Customer List

---

## ?? **Backend Validation (EmployeeService.cs)**

### **Comprehensive Error Messages:**
```csharp
// Name validations
"Employee Name is required"
"Employee Name must be at least 3 characters long"
"Employee Name cannot exceed 50 characters"
"Employee Name can only contain letters, spaces, and dots (.)"

// Department validations
"Department ID is required"
"Department ID must be DEPT01, DEPT02, or DEPT03"

// PAN validations
"PAN is required"
"PAN must be in format: 5 letters + 4 digits + 1 letter (e.g., ABCDE1234F)"
"PAN number 'XXXXX' is already registered with another Employee"
"PAN number 'XXXXX' is already registered as a Customer. Same person cannot be both Employee and Customer."
"You cannot register yourself as an Employee. Please contact your Manager." (future)
```

---

## ?? **Testing Checklist**

### **Manager Dashboard Testing:**
- [ ] Login as Manager (`admin` / `Dummy`)
- [ ] Navigate to "Register Employee" tab (should be visible)
- [ ] Try entering 2-char name ? frontend blocks
- [ ] Try entering numbers in name ? frontend blocks
- [ ] Try entering 8-char PAN ? frontend blocks
- [ ] Try uppercase PAN ? auto-converts to uppercase
- [ ] Submit valid Employee ? success with credentials
- [ ] Try registering same PAN again ? backend error
- [ ] Try registering Customer's PAN as Employee ? backend error

### **Employee Dashboard Testing:**
- [ ] Login as Employee (any department)
- [ ] Check for "Register Employee" tab ? should NOT exist
- [ ] Verify only these tabs visible:
  - Register Customer ?
  - Open Accounts ?
  - Manage Accounts ?
  - Customer List ?

### **Dual Role Prevention Testing:**
- [ ] Register Customer with PAN `ABCDE1234F`
- [ ] Try to register Employee with same PAN ? error
- [ ] Register Employee with PAN `FGHIJ5678K`
- [ ] Try to register Customer with same PAN ? error

---

## ?? **Valid Test Data Set**

```
Employee 1:
  Name: Amit Kumar Sharma
  Department: DEPT01
  PAN: FGHIJ5678K

Employee 2:
  Name: Priya Patel
  Department: DEPT02
  PAN: KLMNO9012P

Employee 3:
  Name: A. K. Singh
  Department: DEPT03
  PAN: MNOPQ3456R

Employee 4 (with customer PAN - should FAIL):
  Name: Test Employee
  Department: DEPT01
  PAN: ABCDE1234F (already a Customer)
  Expected: ? Error
```

---

## ?? **Edge Cases to Test**

1. ? **Name with dots:** `A. K. Sharma` ? Valid
2. ? **Name with multiple spaces:** `Amit  Kumar` ? Valid (trimmed)
3. ? **Department case sensitivity:** `dept01` vs `DEPT01` ? Auto-uppercase
4. ? **PAN case sensitivity:** `abcde1234f` ? Auto-uppercase to `ABCDE1234F`
5. ? **Name with special chars:** `Amit@Kumar` ? Frontend blocks
6. ? **PAN with spaces:** `ABCDE 1234 F` ? Frontend blocks
7. ? **Empty department:** Not selected ? Frontend blocks
8. ? **Exactly 3-char name:** `Ali` ? Valid
9. ? **Exactly 50-char name:** (50-char string) ? Valid
10. ? **51-char name:** Blocked by maxlength

---

## ?? **Pro Testing Tips**

1. **Test as different roles:** Login as Manager, Employee, Customer
2. **Test duplicate PAN scenarios:** Register ? Try duplicate ? Should fail
3. **Test dual role prevention:** Customer PAN ? Try as Employee ? Should fail
4. **Test tab visibility:** Employee should NOT see "Register Employee" tab
5. **Test uppercase conversion:** Enter lowercase PAN ? Should auto-convert
6. **Test name patterns:** Try numbers, special chars ? Should block
7. **Test department dropdown:** Try selecting invalid option ? Should block

---

## ?? **Expected Behavior Summary**

### **Manager Dashboard:**
? Can see "Register Employee" tab
? Can register Employees
? PAN validated (format, uniqueness, dual-role prevention)
? Name validated (min 3 chars, letters only)
? Department validated (DEPT01/02/03 only)

### **Employee Dashboard:**
? Cannot see "Register Employee" tab
? Can register Customers (with PAN validation)
? Can manage accounts
? Can view customer lists

### **PAN Validation:**
? Format: ABCDE1234F (10 chars)
? Unique in Employee table
? **Cannot exist in Customer table** (dual role prevention)
? Auto-uppercase conversion

---

## ?? **Files Updated:**

1. ? `BankApp.Services/EmployeeService.cs` - Backend validations
2. ? `Bank_App/Controllers/DashboardController.cs` - Access control
3. ? `Bank_App/Views/Dashboard/ManagerDashboard.cshtml` - PAN field added
4. ? `Bank_App/Views/Dashboard/EmployeeDashboard.cshtml` - Tab hidden

---

## ?? **All Validations Implemented!**

### **Validation Coverage:**
- ? Employee Name (min 3 chars, max 50 chars, letters only)
- ? Department (DEPT01/02/03 only)
- ? PAN Number (format, uniqueness, dual-role prevention)
- ? Access Control (only Manager can register Employees)
- ? Frontend + Backend validation
- ? User-friendly error messages

**Ready for testing!** ??
