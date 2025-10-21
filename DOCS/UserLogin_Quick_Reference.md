# Quick Reference: UserLogin Table Structure

## Table: UserLogin

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| **UserID** | VARCHAR(50) | **Unique login identifier** | USR00001 |
| **UserName** | VARCHAR(50) | Login username | john1234 |
| **PasswordHash** | VARCHAR(255) | Encrypted password | ******* |
| **Role** | VARCHAR(20) | CUSTOMER/EMPLOYEE/MANAGER | CUSTOMER |
| **ReferenceID** | VARCHAR(50) | **Links to entity table** | MLA00001 |

---

## ID Format Reference

| Entity Type | UserID Format | ReferenceID Format | Example |
|-------------|---------------|-------------------|---------|
| **Customer** | USR##### | MLA##### | USR00001 ? MLA00001 |
| **Employee** | USR##### | 26##### | USR00002 ? 26000001 |
| **Manager** | MGR### | MGR### | MGR001 ? MGR001 |

---

## Sample Data

```sql
-- Customer Login
UserID: USR00001
UserName: john1234
Role: CUSTOMER
ReferenceID: MLA00001  ? Links to Customer.Custid

-- Employee Login
UserID: USR00002
UserName: bob9012
Role: EMPLOYEE
ReferenceID: 26000001  ? Links to Employee.Empid

-- Manager Login
UserID: MGR001
UserName: admin
Role: MANAGER
ReferenceID: MGR001    ? Links to Manager.Mgrid
```

---

## Key Relationships

```
UserLogin.ReferenceID ? Customer.Custid   (when Role = CUSTOMER)
UserLogin.ReferenceID ? Employee.Empid    (when Role = EMPLOYEE)
UserLogin.ReferenceID ? Manager.Mgrid     (when Role = MANAGER)
```

---

## Registration Flow

### Customer Registration:
1. Generate Customer ID: `MLA00001`
2. Create Customer record with ID `MLA00001`
3. Generate UserID: `USR00001`
4. Generate Username: `john1234`
5. Create UserLogin:
   - UserID: `USR00001` (unique login ID)
   - ReferenceID: `MLA00001` (links to Customer)

### Employee Registration:
1. Generate Employee ID: `26000001`
2. Create Employee record with ID `26000001`
3. Generate UserID: `USR00002`
4. Generate Username: `bob9012`
5. Create UserLogin:
   - UserID: `USR00002` (unique login ID)
   - ReferenceID: `26000001` (links to Employee)

---

## Session Variables

After successful login:
```csharp
Session["UserID"] = "USR00001"      // Login identifier
Session["UserName"] = "john1234"    // Username
Session["Role"] = "CUSTOMER"        // User role
Session["ReferenceID"] = "MLA00001" // Entity identifier
```

Use **ReferenceID** to fetch:
- Customer details from Customer table
- Employee details from Employee table
- Manager details from Manager table

---

## Quick SQL Queries

### Get Customer with Login Info:
```sql
SELECT 
    ul.UserID,
    ul.UserName,
    c.Custid,
    c.Custname,
    c.Pan,
    c.PhoneNumber
FROM UserLogin ul
INNER JOIN Customer c ON ul.ReferenceID = c.Custid
WHERE ul.Role = 'CUSTOMER';
```

### Get All Logins with Entity Names:
```sql
SELECT 
    ul.UserID,
    ul.UserName,
    ul.Role,
    ul.ReferenceID,
    COALESCE(c.Custname, e.EmployeeName, m.Mgrname) AS EntityName
FROM UserLogin ul
LEFT JOIN Customer c ON ul.ReferenceID = c.Custid AND ul.Role = 'CUSTOMER'
LEFT JOIN Employee e ON ul.ReferenceID = e.Empid AND ul.Role = 'EMPLOYEE'
LEFT JOIN Manager m ON ul.ReferenceID = m.Mgrid AND ul.Role = 'MANAGER';
```

---

## Important Notes

?? **UserID** and **ReferenceID** are **DIFFERENT**:
- **UserID** = Login system identifier (USR00001)
- **ReferenceID** = Business entity identifier (MLA00001)

? **Always use ReferenceID** to:
- Fetch customer/employee/manager details
- Link to accounts
- Display in dashboards

? **UserID is used only for**:
- Identifying login records
- Audit trails
- System logs

---

## Generated ID Sequences

| Type | Current | Next | Method |
|------|---------|------|--------|
| UserID | USR00005 | USR00006 | `IdGenerator.GenerateUserId()` |
| Customer | MLA00003 | MLA00004 | `IdGenerator.GenerateCustomerId()` |
| Employee | 26000002 | 26000003 | `IdGenerator.GenerateEmployeeId()` |
| Savings | SB00005 | SB00006 | `IdGenerator.GenerateSavingsAccountId()` |
| FD | FD00002 | FD00003 | `IdGenerator.GenerateFixedDepositAccountId()` |
| Loan | LN00001 | LN00002 | `IdGenerator.GenerateLoanAccountId()` |

---

## Troubleshooting

### Problem: Can't find customer details after login
**Solution:** Use `Session["ReferenceID"]`, not `Session["UserID"]`

```csharp
// ? Wrong
string customerId = Session["UserID"];  // This is USR00001

// ? Correct
string customerId = Session["ReferenceID"];  // This is MLA00001
```

### Problem: New registrations still have same UserID and ReferenceID
**Solution:** Make sure you've:
1. Updated `CustomerService.cs` and `EmployeeService.cs`
2. Rebuilt the solution
3. Restarted the application

### Problem: SQL script fails
**Solution:** Check if:
1. UserLogin table has data
2. Backup table already exists (drop it first)
3. You have sufficient permissions

---

**Last Updated:** [Current Date]
**Status:** ? Implemented and Working
