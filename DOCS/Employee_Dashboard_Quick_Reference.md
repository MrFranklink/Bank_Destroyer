# Employee Dashboard - Quick Reference Guide

## ?? Quick Start

### Login as Employee:
1. Use your Employee ID (starts with **26**) or Username
2. Enter password (default: **Dummy**)
3. Dashboard will automatically show department-specific features

---

## ?? Department Features

### DEPT01 - Deposit Management ??
**Tabs Available:**
- Register Customer
- Open Savings
- Open FD
- Process Deposit
- Manage Accounts
- Customer List

**Color Theme:** Blue Gradient

### DEPT02 - Loan Management ??
**Tabs Available:**
- Register Customer
- Open Loan
- Manage Loans
- Customer List

**Color Theme:** Pink/Red Gradient

---

## ?? Permission Quick Check

| I want to... | DEPT01 | DEPT02 |
|--------------|--------|--------|
| Register a customer | ? | ? |
| Open savings account | ? | ? |
| Open fixed deposit | ? | ? |
| Open loan account | ? | ? |
| Process deposit | ? | ? |
| Withdraw money | ? | ? |
| Close savings/FD | ? | ? |
| Close loan | ? | ? |
| View customers | ? | ? |
| Register employee | ? | ? |

---

## ?? Common Actions

### Register a Customer:
```
1. Go to "Register Customer" tab
2. Fill in:
   - Customer Name
   - Date of Birth (18+)
   - PAN Number (Format: ABCD1234)
   - Phone Number
   - Address
3. Click "Register Customer"
4. Note down generated credentials
```

### Open Savings Account (DEPT01):
```
1. Go to "Open Savings" tab
2. Enter:
   - Customer ID (e.g., MLA00001)
   - Initial Deposit (min Rs. 1,000)
3. Click "Open Savings Account"
```

### Open Loan Account (DEPT02):
```
1. Go to "Open Loan" tab
2. Enter:
   - Customer ID
   - Loan Amount (min Rs. 10,000)
   - Start Date
   - Tenure (months)
   - Monthly Salary
3. Click "Sanction Loan"
```

### Process Deposit (DEPT01):
```
1. Go to "Process Deposit" tab
2. Enter:
   - Savings Account ID (e.g., SB00001)
   - Deposit Amount (min Rs. 100)
3. Click "Process Deposit"
```

---

## ?? Common Issues

### "Access Denied" Message:
**Reason:** You're trying to perform an action not allowed for your department  
**Solution:** Check permission matrix above

### "Customer ID not found":
**Reason:** Customer doesn't exist or wrong ID  
**Solution:** Check customer list or register customer first

### "Account already exists":
**Reason:** Customer already has this type of account  
**Solution:** Use existing account or close old account first

### "Minimum deposit not met":
**Reason:** Amount below minimum requirement  
**Solution:** Increase deposit amount (Savings: Rs. 1,000, FD: Rs. 10,000)

---

## ?? Business Rules Reminder

### Savings Account:
- Min deposit: **Rs. 1,000**
- One per customer
- Can deposit/withdraw anytime

### Fixed Deposit:
- Min amount: **Rs. 10,000**
- Interest: **6-8%** (based on tenure)
- Senior citizens: **+0.5%** bonus

### Loan Account:
- Min amount: **Rs. 10,000**
- Interest: **9-10%** (based on amount)
- EMI ? **60%** of salary
- Senior citizens: Max **Rs. 1L**

### Deposit:
- Min amount: **Rs. 100**
- Only for savings accounts
- Instant update

---

## ?? Need Help?

### Check Your Department:
Look at the top-right badge on your dashboard

### View Customers:
Click "Customer List" tab to see all registered customers

### Console Debug:
Press **F12** in browser ? Console tab to see session info

---

## ?? Support

For issues not resolved by this guide:
- Contact your Manager
- Check detailed documentation: `DOCS/Employee_Dashboard_Implementation.md`
- Review error messages carefully

---

**Remember:** 
- Always verify customer ID before operations
- Note down credentials when registering customers
- Check minimum amounts before submitting forms
- Only managers can withdraw money or delete records
