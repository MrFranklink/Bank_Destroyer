# ? Account Opening Validation - Complete Testing Guide

## ?? **All Business Rules Validated**

This document covers validation for ALL account types:
1. ? **Savings Account**
2. ? **Fixed Deposit (FD) Account**
3. ? **Loan Account**

---

## ?? **Validation Rules Summary**

### **1?? Savings Account**

| Rule | Validation | Location |
|------|-----------|----------|
| **Minimum Deposit** | Rs. 1,000 | ? Frontend + Backend |
| **Balance Cannot Fall Below** | Rs. 1,000 | ? Backend (Withdraw) |
| **Min Deposit/Withdrawal** | Rs. 100 | ? Backend |
| **Auto-generated Account Number** | SB00001 format | ? Backend |
| **One Account Per Customer** | Checked in DB | ? Backend |
| **Customer ID Format** | MLA00001 | ? Frontend Pattern |

---

### **2?? Fixed Deposit Account**

| Rule | Validation | Location |
|------|-----------|----------|
| **Minimum Deposit** | Rs. 10,000 | ? Frontend + Backend |
| **Start Date** | Today or future only | ? Frontend + Backend |
| **Tenure Range** | 1-360 months | ? Frontend + Backend |
| **Interest Rates** | 6%, 7%, 8% (by tenure) | ? Backend Auto-calc |
| **Senior Citizen Bonus** | +0.5% | ? Backend Auto-calc |
| **Auto-generated Account Number** | FD00001 format | ? Backend |
| **Maturity Amount** | Displayed on success | ? Backend Calc |
| **Customer ID Format** | MLA00001 | ? Frontend Pattern |

**Interest Rate Logic:**
```
Tenure ? 12 months: 6%
Tenure 13-24 months: 7%
Tenure > 24 months: 8%
Senior Citizen: +0.5% bonus on all
```

---

### **3?? Loan Account**

| Rule | Validation | Location |
|------|-----------|----------|
| **Minimum Loan** | Rs. 10,000 | ? Frontend + Backend |
| **Start Date** | Today or future only | ? Frontend + Backend |
| **Tenure Range** | 1-360 months | ? Frontend + Backend |
| **Monthly Salary** | Min Rs. 1,000 | ? Frontend + Backend |
| **EMI ? 60% Salary** | Auto-validated | ? Backend |
| **Senior Max Loan** | Rs. 1 lakh (100,000) | ? Backend |
| **Interest Rates** | 10%, 9.5%, 9% (by amount) | ? Backend Auto-calc |
| **Senior Citizen Rate** | Fixed 9.5% | ? Backend |
| **Auto-generated Account Number** | LN00001 format | ? Backend |
| **EMI Displayed** | Shown on success | ? Backend Calc |
| **Customer ID Format** | MLA00001 | ? Frontend Pattern |

**Interest Rate Logic:**
```
Amount ? Rs. 5,00,000: 10%
Amount Rs. 5,00,001 - 10,00,000: 9.5%
Amount > Rs. 10,00,000: 9%
Senior Citizen: 9.5% (fixed, regardless of amount)
```

---

## ?? **Test Cases**

### ? **SAVINGS ACCOUNT TEST CASES**

#### **TC1: Valid Savings Account**
```
Customer ID: MLA00001
Initial Deposit: Rs. 5,000
```
**Expected:** ? Success - Account created

---

#### **TC2: Below Minimum Deposit**
```
Customer ID: MLA00001
Initial Deposit: Rs. 500
```
**Expected:** ? Error - "Minimum deposit for Savings Account is Rs. 1,000"

---

#### **TC3: Invalid Customer ID Format**
```
Customer ID: ABC123
Initial Deposit: Rs. 5,000
```
**Expected:** ? Frontend blocks - Pattern mismatch (MLA00001 required)

---

#### **TC4: Customer Not Found**
```
Customer ID: MLA99999
Initial Deposit: Rs. 5,000
```
**Expected:** ? Error - "Customer ID 'MLA99999' not found in the system"

---

#### **TC5: Duplicate Savings Account**
```
Customer ID: MLA00001 (already has savings account)
Initial Deposit: Rs. 5,000
```
**Expected:** ? Error - "Customer already has a Savings Account. Only one savings account allowed per customer."

---

### ? **FIXED DEPOSIT TEST CASES**

#### **TC6: Valid FD Account**
```
Customer ID: MLA00001
Amount: Rs. 50,000
Start Date: 2025-01-15
Tenure: 24 months
```
**Expected:** ? Success - Account created with 7% interest rate

---

#### **TC7: Below Minimum Amount**
```
Customer ID: MLA00001
Amount: Rs. 5,000
Start Date: 2025-01-15
Tenure: 12 months
```
**Expected:** ? Error - "Minimum deposit for Fixed Deposit is Rs. 10,000"

---

#### **TC8: Past Start Date**
```
Customer ID: MLA00001
Amount: Rs. 50,000
Start Date: 2023-01-01 (past date)
Tenure: 12 months
```
**Expected:** ? Error - "Start date cannot be in the past. Please select today or a future date."

---

#### **TC9: Tenure Too Long**
```
Customer ID: MLA00001
Amount: Rs. 50,000
Start Date: 2025-01-15
Tenure: 400 months
```
**Expected:** ? Error - "Maximum tenure is 360 months (30 years)"

---

#### **TC10: Senior Citizen FD**
```
Customer ID: MLA00002 (DOB: 1960-01-01, 64 years old)
Amount: Rs. 50,000
Start Date: 2025-01-15
Tenure: 24 months
```
**Expected:** ? Success - Account created with 7.5% interest rate (7% + 0.5% bonus)

---

#### **TC11: Short Tenure FD**
```
Customer ID: MLA00001
Amount: Rs. 50,000
Start Date: 2025-01-15
Tenure: 6 months
```
**Expected:** ? Success - Account created with 6% interest rate

---

#### **TC12: Long Tenure FD**
```
Customer ID: MLA00001
Amount: Rs. 50,000
Start Date: 2025-01-15
Tenure: 36 months
```
**Expected:** ? Success - Account created with 8% interest rate

---

### ? **LOAN ACCOUNT TEST CASES**

#### **TC13: Valid Loan Account**
```
Customer ID: MLA00001
Loan Amount: Rs. 3,00,000
Start Date: 2025-01-15
Tenure: 60 months
Monthly Salary: Rs. 50,000
```
**Expected:** ? Success - Loan sanctioned with 10% interest, EMI ? Rs. 6,373

---

#### **TC14: Below Minimum Loan**
```
Customer ID: MLA00001
Loan Amount: Rs. 5,000
Start Date: 2025-01-15
Tenure: 12 months
Monthly Salary: Rs. 20,000
```
**Expected:** ? Error - "Minimum loan amount is Rs. 10,000"

---

#### **TC15: EMI Exceeds 60% Salary**
```
Customer ID: MLA00001
Loan Amount: Rs. 10,00,000
Start Date: 2025-01-15
Tenure: 12 months
Monthly Salary: Rs. 30,000
```
**Expected:** ? Error - "EMI amount (Rs. 87,916.05) exceeds 60% of monthly salary (Rs. 18,000.00). Please reduce loan amount or increase tenure."

---

#### **TC16: Senior Citizen Loan (Valid)**
```
Customer ID: MLA00002 (Senior)
Loan Amount: Rs. 80,000
Start Date: 2025-01-15
Tenure: 24 months
Monthly Salary: Rs. 30,000
```
**Expected:** ? Success - Loan sanctioned with 9.5% interest rate

---

#### **TC17: Senior Citizen Loan Exceeds Limit**
```
Customer ID: MLA00002 (Senior)
Loan Amount: Rs. 1,50,000
Start Date: 2025-01-15
Tenure: 24 months
Monthly Salary: Rs. 50,000
```
**Expected:** ? Error - "Senior citizens cannot be sanctioned a loan greater than Rs. 1 lakh (Rs. 100,000)"

---

#### **TC18: Large Loan (9% Interest)**
```
Customer ID: MLA00001
Loan Amount: Rs. 15,00,000
Start Date: 2025-01-15
Tenure: 120 months
Monthly Salary: Rs. 1,00,000
```
**Expected:** ? Success - Loan sanctioned with 9% interest rate

---

#### **TC19: Medium Loan (9.5% Interest)**
```
Customer ID: MLA00001
Loan Amount: Rs. 7,00,000
Start Date: 2025-01-15
Tenure: 60 months
Monthly Salary: Rs. 80,000
```
**Expected:** ? Success - Loan sanctioned with 9.5% interest rate

---

#### **TC20: Past Start Date**
```
Customer ID: MLA00001
Loan Amount: Rs. 50,000
Start Date: 2023-01-01 (past)
Tenure: 12 months
Monthly Salary: Rs. 20,000
```
**Expected:** ? Error - "Start date cannot be in the past. Please select today or a future date."

---

#### **TC21: Invalid Monthly Salary**
```
Customer ID: MLA00001
Loan Amount: Rs. 50,000
Start Date: 2025-01-15
Tenure: 12 months
Monthly Salary: Rs. 500
```
**Expected:** ? Error - "Please enter a valid monthly salary (minimum Rs. 1,000)"

---

## ?? **Frontend Validation Summary**

### **HTML5 Patterns:**

**Customer ID:**
```html
<input name="customerId" 
       pattern="MLA[0-9]{5}"
       title="Format: MLA followed by 5 digits"
       maxlength="8"
       style="text-transform: uppercase;"
       required />
```

**Savings Initial Deposit:**
```html
<input name="initialDeposit" 
       type="number" 
       step="0.01" 
       min="1000"
       required />
```

**FD Amount:**
```html
<input name="amount" 
       type="number" 
       step="0.01" 
       min="10000"
       required />
```

**FD/Loan Start Date:**
```html
<input name="startDate" 
       type="date" 
       min="@DateTime.Now.ToString("yyyy-MM-dd")"
       required />
```

**Tenure:**
```html
<input name="tenureMonths" 
       type="number" 
       min="1"
       max="360"
       required />
```

**Loan Amount:**
```html
<input name="loanAmount" 
       type="number" 
       step="0.01" 
       min="10000"
       required />
```

**Monthly Salary:**
```html
<input name="monthlySalary" 
       type="number" 
       step="0.01" 
       min="1000"
       required />
```

---

## ?? **Backend Validation Summary**

### **SavingsAccountService:**
```csharp
? Customer ID required
? Customer exists check
? Min deposit Rs. 1,000
? One account per customer check
? Withdrawal min Rs. 100
? Balance cannot fall below Rs. 1,000
```

### **FixedDepositAccountService:**
```csharp
? Customer ID required
? Customer exists check
? Min amount Rs. 10,000
? Start date not in past
? Tenure 1-360 months
? Interest rate auto-calculated
? Senior citizen +0.5% bonus
? Maturity amount calculated
```

### **LoanAccountService:**
```csharp
? Customer ID required
? Customer exists check
? Min loan Rs. 10,000
? Start date not in past
? Tenure 1-360 months
? Monthly salary min Rs. 1,000
? EMI ? 60% of salary
? Senior max Rs. 1 lakh
? Interest rate auto-calculated
? Senior fixed rate 9.5%
? EMI calculated and displayed
```

---

## ?? **Testing Checklist**

### **For Each Account Type:**

- [ ] **Valid registration** with all correct values
- [ ] **Below minimum** amount/deposit
- [ ] **Invalid Customer ID** format
- [ ] **Customer not found** in database
- [ ] **Past start date** (should fail)
- [ ] **Future start date** (should work)
- [ ] **Today as start date** (should work)
- [ ] **Tenure = 0** (should fail)
- [ ] **Tenure = 361** (should fail for FD/Loan)
- [ ] **Senior citizen** validation
- [ ] **Empty fields** (should block)

### **Loan-Specific:**
- [ ] **EMI > 60% salary** (should fail)
- [ ] **Senior loan > Rs. 1L** (should fail)
- [ ] **Salary < Rs. 1,000** (should fail)
- [ ] **Interest rate tiers** (10%, 9.5%, 9%)

### **Savings-Specific:**
- [ ] **Duplicate account** for same customer (should fail)
- [ ] **Withdrawal leaving < Rs. 1,000** (should fail)

---

## ? **All Validations Implemented!**

### **Files Updated:**
1. ? `BankApp.Services/SavingsAccountService.cs` - Backend validations
2. ? `BankApp.Services/FixedDepositAccountService.cs` - Backend validations + date check
3. ? `BankApp.Services/LoanAccountService.cs` - Backend validations + date check
4. ? `Bank_App/Views/Dashboard/ManagerDashboard.cshtml` - Frontend HTML5 validation
5. ? `Bank_App/Views/Dashboard/EmployeeDashboard.cshtml` - Frontend HTML5 validation

### **Build Status:** ? SUCCESS

---

## ?? **Ready for Testing!**

All account opening forms now have:
- ? **Comprehensive frontend validation** (HTML5 patterns)
- ? **Robust backend validation** (business rules)
- ? **User-friendly error messages**
- ? **Date validation** (no past dates)
- ? **Customer ID format** (MLA00001)
- ? **All business rules** from requirements document

**Test away!** ??
