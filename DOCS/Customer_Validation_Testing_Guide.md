# ? Customer Registration - Complete Validation Testing Guide

## ?? **Validation Rules Summary**

| Field | Rules | Example Valid | Example Invalid |
|-------|-------|---------------|-----------------|
| **Customer Name** | • Required<br>• Min 3, Max 50 chars<br>• Only letters, spaces, dots (.) | `John Doe`<br>`A. K. Sharma` | `John123` ?<br>`AB` ?<br>`John@Doe` ? |
| **Date of Birth** | • Required<br>• Age 18-100 years<br>• Not future date | `2000-01-15` (24 years old) | `2010-01-01` ? (14 years)<br>`2025-01-01` ? (future) |
| **PAN Number** | • Required<br>• Format: ABCDE1234F<br>• Unique across Customer & Employee | `ABCDE1234F`<br>`BNZAA2318J` | `ABCD1234` ? (8 chars)<br>`ABC123456` ? (wrong format) |
| **Phone Number** | • **REQUIRED** ?<br>• Exactly 10 digits<br>• Must start with 6, 7, 8, or 9<br>• **Shared allowed** | `9876543210`<br>`7001234567` | `1234567890` ? (starts with 1)<br>`987654321` ? (9 digits) |
| **Address** | • **REQUIRED** ?<br>• Min 10, Max 100 chars | `123 Main Street, City` | `Short` ? (< 10 chars) |

---

## ?? **Test Cases**

### ? **Test Case 1: Valid Registration**
```
Customer Name: Rajesh Kumar
DOB: 1990-05-15 (34 years old)
PAN: ABCDE1234F
Phone: 9876543210
Address: 123 MG Road, Bangalore, Karnataka 560001
```
**Expected:** ? Success - Customer registered

---

### ? **Test Case 2: Invalid Name (Numbers)**
```
Customer Name: John123
DOB: 1990-05-15
PAN: ABCDE1234F
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "Customer Name can only contain letters, spaces, and dots (.)"

---

### ? **Test Case 3: Invalid Name (Too Short)**
```
Customer Name: AB
DOB: 1990-05-15
PAN: ABCDE1234F
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "Customer Name must be at least 3 characters long"

---

### ? **Test Case 4: Under 18 Years**
```
Customer Name: Young Person
DOB: 2010-01-01 (14 years old)
PAN: ABCDE1234F
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "Customer must be at least 18 years old"

---

### ? **Test Case 5: Future Date**
```
Customer Name: Time Traveler
DOB: 2025-01-01
PAN: ABCDE1234F
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "Date of Birth cannot be in the future"

---

### ? **Test Case 6: Age > 100 Years**
```
Customer Name: Very Old Person
DOB: 1900-01-01 (124 years old)
PAN: ABCDE1234F
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "Please enter a valid Date of Birth (age cannot exceed 100 years)"

---

### ? **Test Case 7: Invalid PAN (Wrong Format)**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: ABCD1234 (8 characters)
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "PAN must be in format: 5 letters + 4 digits + 1 letter"

---

### ? **Test Case 8: Duplicate PAN**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: ABCDE1234F (already registered)
Phone: 9876543210
Address: 123 Main Street
```
**Expected:** ? Error - "PAN number 'ABCDE1234F' is already registered with another customer"

---

### ? **Test Case 9: Invalid Phone (Starts with 5)**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: FGHIJ5678K
Phone: 5876543210
Address: 123 Main Street
```
**Expected:** ? Error - "Phone Number must be a valid 10-digit Indian mobile number starting with 6, 7, 8, or 9"

---

### ? **Test Case 10: Invalid Phone (9 Digits)**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: FGHIJ5678K
Phone: 987654321
Address: 123 Main Street
```
**Expected:** ? Error - "Phone Number must be a valid 10-digit Indian mobile number starting with 6, 7, 8, or 9"

---

### ? **Test Case 11: Invalid Phone (11 Digits)**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: FGHIJ5678K
Phone: 98765432101
Address: 123 Main Street
```
**Expected:** ? Error - HTML5 validation prevents (maxlength=10)

---

### ? **Test Case 12: Empty Phone**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: FGHIJ5678K
Phone: (empty)
Address: 123 Main Street
```
**Expected:** ? Error - "Phone Number is required"

---

### ? **Test Case 13: Address Too Short**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: FGHIJ5678K
Phone: 9876543210
Address: Short
```
**Expected:** ? Error - "Address must be at least 10 characters long"

---

### ? **Test Case 14: Empty Address**
```
Customer Name: Test User
DOB: 1990-05-15
PAN: FGHIJ5678K
Phone: 9876543210
Address: (empty)
```
**Expected:** ? Error - "Address is required"

---

### ? **Test Case 15: Shared Phone Number**
```
Register Customer 1:
  Name: First Customer
  Phone: 9876543210
  (Other valid fields)

Register Customer 2:
  Name: Second Customer
  Phone: 9876543210 (same as Customer 1)
  (Other valid fields)
```
**Expected:** ? Both succeed - Phone sharing allowed

---

## ?? **Frontend Validation (HTML5)**

### **Instant Feedback:**
- ? **Name pattern:** Only allows letters, spaces, dots
- ? **PAN pattern:** Auto-uppercase, enforces ABCDE1234F format
- ? **Phone pattern:** Only 10 digits starting with 6-9
- ? **DOB range:** Date picker limited to 18-100 years
- ? **Address min length:** Shows error if < 10 chars

### **Browser Validation Messages:**
```
Name: "Please match the requested format"
PAN: "Please match the requested format"
Phone: "Please match the requested format"
DOB: "Please enter a valid date"
Address: "Please fill out this field"
```

---

## ?? **Backend Validation (C# Service Layer)**

### **Comprehensive Error Messages:**
```csharp
// Name validations
"Customer Name is required"
"Customer Name must be at least 3 characters long"
"Customer Name cannot exceed 50 characters"
"Customer Name can only contain letters, spaces, and dots (.)"

// PAN validations
"PAN is required"
"PAN must be in format: 5 letters + 4 digits + 1 letter (e.g., ABCDE1234F)"
"PAN number 'XXXXX' is already registered with another customer"
"PAN number 'XXXXX' is already registered as an employee"

// Phone validations
"Phone Number is required"
"Phone Number must be a valid 10-digit Indian mobile number starting with 6, 7, 8, or 9 (e.g., 9876543210)"

// Address validations
"Address is required"
"Address must be at least 10 characters long"
"Address cannot exceed 100 characters"

// DOB validations
"Date of Birth cannot be in the future"
"Customer must be at least 18 years old"
"Please enter a valid Date of Birth (age cannot exceed 100 years)"
```

---

## ?? **Testing Checklist**

### **Frontend Testing:**
- [ ] Try entering numbers in name field ? blocked by pattern
- [ ] Try entering < 10 chars in address ? shows error
- [ ] Try selecting future DOB ? blocked by max date
- [ ] Try entering 9-digit phone ? blocked by pattern
- [ ] Try entering phone starting with 5 ? blocked by pattern
- [ ] Try uppercase PAN ? auto-converts to uppercase

### **Backend Testing:**
- [ ] Submit name with 2 chars ? backend error
- [ ] Submit duplicate PAN ? backend error
- [ ] Submit DOB with age 17 ? backend error
- [ ] Submit DOB with age 101 ? backend error
- [ ] Submit empty phone ? backend error
- [ ] Submit phone starting with 5 ? backend error
- [ ] Submit address with 9 chars ? backend error
- [ ] Submit same phone for 2 customers ? both succeed

---

## ?? **Valid Test Data Set**

```
Customer 1:
  Name: Amit Kumar Sharma
  DOB: 1985-03-15
  PAN: ABCDE1234F
  Phone: 9876543210
  Address: 123 MG Road, Bangalore, Karnataka 560001

Customer 2:
  Name: Priya Patel
  DOB: 1992-07-22
  PAN: BNZAA2318J
  Phone: 9876543210 (same as Customer 1 - allowed)
  Address: 456 Park Street, Mumbai, Maharashtra 400001

Customer 3:
  Name: Rahul Verma
  DOB: 2000-11-30
  PAN: CDEFG5678K
  Phone: 7001234567
  Address: 789 Ring Road, Delhi, NCR 110001

Senior Citizen:
  Name: A. K. Singh
  DOB: 1960-01-10 (64 years old - senior)
  PAN: KLMNO9012P
  Phone: 6543210987
  Address: 321 Civil Lines, Lucknow, Uttar Pradesh 226001
```

---

## ?? **Edge Cases to Test**

1. ? **Name with dots:** `A. K. Sharma` ? Valid
2. ? **Name with multiple spaces:** `John  Doe` ? Valid (trimmed)
3. ? **Exactly 18 years old:** `2006-[TODAY]` ? Valid
4. ? **Exactly 100 years old:** `1924-[TODAY]` ? Valid
5. ? **Phone starting with 6:** `6543210987` ? Valid
6. ? **Phone starting with 9:** `9876543210` ? Valid
7. ? **Address exactly 10 chars:** `1234567890` ? Valid
8. ? **Address exactly 100 chars:** (100-char string) ? Valid
9. ? **Name with special chars:** `John@Doe` ? Invalid
10. ? **Phone with letters:** `98765ABC10` ? Blocked by HTML5

---

## ?? **Pro Testing Tips**

1. **Use Browser DevTools Console:** Check for JavaScript errors
2. **Test in different browsers:** Chrome, Edge, Firefox
3. **Test with keyboard navigation:** Tab through fields
4. **Test copy-paste:** Paste invalid data into fields
5. **Test auto-fill:** Use browser autofill feature
6. **Test mobile view:** Responsive design check
7. **Test network errors:** Disconnect internet during submit

---

## ?? **Test Results Template**

```
Test Date: [DATE]
Tester: [NAME]
Environment: [DEV/QA/PROD]

| Test Case | Input | Expected | Actual | Status |
|-----------|-------|----------|--------|--------|
| TC1 - Valid Registration | Valid data | Success | Success | PASS ? |
| TC2 - Invalid Name | John123 | Error | Error | PASS ? |
| TC3 - Short Name | AB | Error | Error | PASS ? |
| ... | ... | ... | ... | ... |

Overall Pass Rate: X/Y (XX%)
Issues Found: [List any bugs]
```

---

## ?? **All Validations Implemented!**

### **Files Updated:**
1. ? `BankApp.Services/CustomerService.cs` - Backend validations
2. ? `Bank_App/Views/Dashboard/ManagerDashboard.cshtml` - Frontend validations
3. ? `Bank_App/Views/Dashboard/EmployeeDashboard.cshtml` - DEPT01 & DEPT02 forms

### **Validation Coverage:**
- ? Customer Name (min/max length, character validation)
- ? Date of Birth (age 18-100, not future)
- ? PAN Number (format, uniqueness)
- ? Phone Number (required, 10-digit Indian format, shared allowed)
- ? Address (required, min 10 chars)

**Ready for testing!** ??
