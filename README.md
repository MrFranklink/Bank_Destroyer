# ?? Banking Application - ASP.NET MVC

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-6.0-green.svg)](https://docs.microsoft.com/en-us/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-red.svg)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple.svg)](https://getbootstrap.com/)

A comprehensive **Banking Management System** built with ASP.NET MVC, featuring role-based access control, department-wise permissions, modern UI/UX, and secure password management.

---

## ?? Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Database Schema](#-database-schema)
- [User Roles & Permissions](#-user-roles--permissions)
- [Business Logic](#-business-logic)
- [Security Features](#-security-features)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Usage](#-usage)
- [Screenshots](#-screenshots)
- [Project Structure](#-project-structure)
- [API Reference](#-api-reference)
- [Testing](#-testing)
- [Known Issues](#-known-issues)
- [Future Enhancements](#-future-enhancements)
- [Contributing](#-contributing)
- [License](#-license)
- [Contact](#-contact)

---

## ?? Overview

This Banking Application is a **full-featured web-based banking management system** designed for academic/educational purposes. It demonstrates modern software development practices including:

- **3-Layer Architecture** (Presentation ? Business Logic ? Data Access)
- **Role-Based Access Control** (Manager, Employee, Customer)
- **Department-Based Permissions** (DEPT01: Deposits, DEPT02: Loans)
- **Secure Authentication** (SHA256 Password Hashing)
- **Modern UI/UX** (Gradient Backgrounds, Smooth Animations)
- **Self-Service Banking** (Customer Deposit/Withdraw)

### ?? Project Type
- **Academic Final Year Project** (College/University)
- **Complexity Level:** Beginner to Intermediate (?????)
- **Suitable For:** Learning ASP.NET MVC, Entity Framework, Banking Domain

---

## ? Features

### ?? Authentication & Authorization
- ? Secure login with username/password
- ? Session-based authentication
- ? Role-based authorization (Manager, Employee, Customer)
- ? **SHA256 password hashing** with Base64 encoding
- ? **Change password** functionality for all users
- ? Backward compatibility (supports plain text during migration)
- ? Logout with session cleanup

### ?? User Management
- ? **Auto-generated IDs:**
  - Customer: `MLA00001`, `MLA00002`, ...
  - Employee: `2600001`, `2600002`, ...
  - Manager: `MGR001`, `MGR002`, ...
- ? Auto-generated usernames (lowercase of name)
- ? Default password: `Dummy` (displayed once after registration)
- ? Credentials display in styled box
- ? PAN number uniqueness validation (cross-table)

### ?? Account Management

#### Savings Account
- Minimum deposit: **Rs. 1,000**
- Limit: **One account per customer**
- Account ID: `SB00001`, `SB00002`, ...
- Minimum balance: **Rs. 1,000** (maintained)

#### Fixed Deposit
- Minimum: **Rs. 10,000**
- **Interest Rates:**
  - < 1 year: **6.0%**
  - 1-2 years: **7.0%**
  - > 2 years: **8.0%**
  - Senior Citizen Bonus: **+0.5%**
- Account ID: `FD00001`, `FD00002`, ...
- Maturity calculation: Compound interest

#### Loan Account
- Minimum: **Rs. 10,000**
- **Interest Rates:**
  - < Rs. 5 lakhs: **10.0%**
  - Rs. 5-10 lakhs: **9.5%**
  - > Rs. 10 lakhs: **9.0%**
- **Special Rules:**
  - EMI ? **60% of monthly salary**
  - Senior Citizens: max **Rs. 1 lakh**, **9.5%** rate
- Account ID: `LA00001`, `LA00002`, ...

### ?? Transaction Management
- ? **Deposit:** Manager, DEPT01 employees, Customers
- ? **Withdraw:** Manager, Customers
- ? Minimum transaction: **Rs. 100**
- ? Transaction history tracking
- ? Real-time balance updates

### ?? Validation Rules
- ? **PAN Validation:** 4 letters + 4 digits (e.g., ABCD1234)
- ? **PAN Uniqueness:** Cross-table check (Customer, Employee, Manager)
- ? **Age Validation:** Minimum 18 years
- ? **Senior Citizen:** Age > 60 years
- ? **EMI Validation:** EMI ? 60% of salary
- ? **Account Limits:** One savings account per customer

### ?? Modern UI/UX
- ? **Role-Specific Gradients:**
  - Manager: Purple (#667eea ? #764ba2)
  - Employee DEPT01: Blue (#667eea ? #00c9ff)
  - Employee DEPT02: Pink (#f857a6 ? #ff5858)
  - Customer: Green (#11998e ? #38ef7d)
- ? Smooth animations (fade-in, slide-up)
- ? Responsive design (Bootstrap 5)
- ? Dynamic tabs with role-based content
- ? Bootstrap Icons integration
- ? Hover effects and transitions

---

## ??? Technology Stack

### Backend
- **Framework:** ASP.NET MVC 5 (.NET Framework 4.8)
- **ORM:** Entity Framework 6.0 (Database-First)
- **Language:** C# 7.3
- **Database:** SQL Server 2019
- **Authentication:** FormsAuthentication (Session-based)

### Frontend
- **UI Framework:** Bootstrap 5.3
- **Icons:** Bootstrap Icons
- **CSS:** Custom gradients, animations, responsive design
- **JavaScript:** Vanilla JS (form validation, AJAX)

### Tools & Libraries
- **IDE:** Visual Studio 2022
- **Version Control:** Git + GitHub
- **Database Tool:** SQL Server Management Studio (SSMS)
- **Hashing:** SHA256 (System.Security.Cryptography)

---

## ??? Architecture

### 3-Layer Architecture

```
???????????????????????????????????????????????
?         Presentation Layer (MVC)            ?
?  - Controllers (AuthController, Dashboard)  ?
?  - Views (Razor, cshtml)                    ?
?  - Session Management                       ?
???????????????????????????????????????????????
                    ?
???????????????????????????????????????????????
?       Business Logic Layer (Services)       ?
?  - AuthService (Login, Password)            ?
?  - CustomerService (Registration)           ?
?  - EmployeeService (Dept Logic)             ?
?  - AccountServices (Savings, FD, Loan)      ?
?  - TransactionService (Deposit, Withdraw)   ?
???????????????????????????????????????????????
                    ?
???????????????????????????????????????????????
?       Data Access Layer (Repositories)      ?
?  - UserLoginRepository                      ?
?  - CustomerRepository                       ?
?  - EmployeeRepository                       ?
?  - AccountRepositories                      ?
?  - Entity Framework Context                 ?
???????????????????????????????????????????????
                    ?
???????????????????????????????????????????????
?             Database (SQL Server)           ?
?  - UserLogin, Customer, Employee            ?
?  - Account, SavingsAccount, FD, Loan        ?
?  - SavingsTransaction                       ?
???????????????????????????????????????????????
```

---

## ??? Database Schema

### Core Tables

#### UserLogin (Authentication)
```sql
UserID VARCHAR(20) PRIMARY KEY
UserName VARCHAR(50) UNIQUE NOT NULL
PasswordHash VARCHAR(500) NOT NULL
Role VARCHAR(20) NOT NULL -- 'MANAGER', 'EMPLOYEE', 'CUSTOMER'
ReferenceID VARCHAR(20) NULL -- MGR001, 2600001, MLA00001
```

#### Customer
```sql
Custid VARCHAR(20) PRIMARY KEY -- MLA00001
Custname VARCHAR(50) NOT NULL
Pan VARCHAR(8) UNIQUE NOT NULL -- ABCD1234
DOB DATE NULL
PhoneNumber VARCHAR(15) NULL
Address VARCHAR(100) NULL
```

#### Employee
```sql
Empid VARCHAR(20) PRIMARY KEY -- 2600001
EmployeeName VARCHAR(50) NOT NULL
DeptId VARCHAR(10) NOT NULL -- DEPT01, DEPT02, DEPT03
Pan VARCHAR(8) UNIQUE NOT NULL
```

#### Account (Base Table)
```sql
AccountID VARCHAR(20) PRIMARY KEY -- SB00001, FD00001, LA00001
CustomerID VARCHAR(20) FOREIGN KEY
AccountType VARCHAR(20) -- 'SAVING', 'FIXED-DEPOSIT', 'LOAN'
OpenDate DATE NOT NULL
Status VARCHAR(10) -- 'OPEN', 'CLOSED'
OpenedBy VARCHAR(20) -- Manager/Employee ID
OpenedByRole VARCHAR(20) -- 'MANAGER', 'EMPLOYEE'
```

#### SavingsAccount
```sql
AccountID VARCHAR(20) PRIMARY KEY, FOREIGN KEY
Balance DECIMAL(18,2) NOT NULL
```

#### FixedDepositAccount
```sql
AccountID VARCHAR(20) PRIMARY KEY, FOREIGN KEY
Amount DECIMAL(18,2) NOT NULL
InterestRate DECIMAL(5,2) NOT NULL
StartDate DATE NOT NULL
EndDate DATE NOT NULL
MaturityAmount DECIMAL(18,2) NOT NULL
```

#### LoanAccount
```sql
AccountID VARCHAR(20) PRIMARY KEY, FOREIGN KEY
LoanAmount DECIMAL(18,2) NOT NULL
InterestRate DECIMAL(5,2) NOT NULL
StartDate DATE NOT NULL
Tenure INT NOT NULL -- in months
EMI DECIMAL(18,2) NOT NULL
```

#### SavingsTransaction
```sql
Transationid INT PRIMARY KEY IDENTITY
AccountID VARCHAR(20) FOREIGN KEY
Transationdate DATETIME NOT NULL
Transactiontype VARCHAR(20) -- 'DEPOSIT', 'WITHDRAW'
Amount DECIMAL(18,2) NOT NULL
```

### Entity Relationship Diagram

```
UserLogin ???
            ??? Customer ??? Account ????? SavingsAccount ??? SavingsTransaction
            ?                          ??? FixedDepositAccount
            ??? Employee               ??? LoanAccount
            ??? Manager
```

---

## ?? User Roles & Permissions

### ?? MANAGER (Full Access)

| Feature | Permission |
|---------|------------|
| Register Customers | ? Yes |
| Register Employees | ? Yes |
| Open Savings Accounts | ? Yes |
| Open Fixed Deposits | ? Yes |
| Open Loan Accounts | ? Yes |
| Process Deposits | ? Yes |
| Process Withdrawals | ? Yes |
| Close Any Account | ? Yes |
| View All Customers | ? Yes |
| View All Employees | ? Yes |
| Delete Customers | ? Yes |
| Delete Employees | ? Yes |
| View Transaction History | ? Yes |
| Change Password | ? Yes |

### ?? EMPLOYEE - DEPT01 (Deposit Management)

| Feature | Permission |
|---------|------------|
| Register Customers | ? Yes |
| Register Employees | ? No |
| Open Savings Accounts | ? Yes |
| Open Fixed Deposits | ? Yes |
| Open Loan Accounts | ? No |
| Process Deposits | ? Yes |
| Process Withdrawals | ? No |
| Close Savings/FD Accounts | ? Yes |
| Close Loan Accounts | ? No |
| View Customer List | ? Yes |
| Delete Staff | ? No |
| Change Password | ? Yes |

### ?? EMPLOYEE - DEPT02 (Loan Management)

| Feature | Permission |
|---------|------------|
| Register Customers | ? Yes |
| Register Employees | ? No |
| Open Savings Accounts | ? No |
| Open Fixed Deposits | ? No |
| Open Loan Accounts | ? Yes |
| Process Deposits | ? No |
| Process Withdrawals | ? No |
| Close Savings/FD Accounts | ? No |
| Close Loan Accounts | ? Yes |
| View Customer List | ? Yes |
| Delete Staff | ? No |
| Change Password | ? Yes |

### ?? CUSTOMER (Self-Service)

| Feature | Permission |
|---------|------------|
| View Own Profile | ? Yes |
| View Own Accounts | ? Yes |
| Deposit to Savings | ? Yes |
| Withdraw from Savings | ? Yes |
| View Transaction History | ? Yes (Own) |
| Open Accounts | ? No (Must visit branch) |
| Close Accounts | ? No |
| View Other Customers | ? No |
| Change Password | ? Yes |

---

## ?? Business Logic

### Interest Rate Calculation (Fixed Deposit)

```csharp
// File: BankApp.Services/FixedDepositAccountService.cs

double rate;
if (tenureMonths < 12) 
    rate = 6.0;
else if (tenureMonths <= 24) 
    rate = 7.0;
else 
    rate = 8.0;

// Senior citizen bonus
if (customer.DOB.HasValue)
{
    int age = (int)((DateTime.Now - customer.DOB.Value).TotalDays / 365.25);
    if (age > 60) 
        rate += 0.5; // 8.5% for seniors
}
```

### Maturity Amount Calculation

```csharp
// Compound interest formula
double maturityAmount = amount * Math.Pow(1 + interestRate / 100, tenureMonths / 12.0);
```

### EMI Calculation (Loan)

```csharp
// File: BankApp.Services/LoanAccountService.cs

double monthlyRate = interestRate / 12 / 100;
double emi = loanAmount * monthlyRate * Math.Pow(1 + monthlyRate, tenure) 
           / (Math.Pow(1 + monthlyRate, tenure) - 1);

// Validation: EMI ? 60% of salary
double maxEMI = monthlySalary * 0.6;
if (emi > maxEMI)
    return Error($"EMI (Rs. {emi:N2}) exceeds 60% of salary (Rs. {maxEMI:N2})");
```

### Loan Interest Rate Logic

```csharp
if (loanAmount < 500000) 
    rate = 10.0;
else if (loanAmount <= 1000000) 
    rate = 9.5;
else 
    rate = 9.0;

// Senior citizen override
if (isSeniorCitizen)
{
    if (loanAmount > 100000)
        return Error("Senior citizens can take max Rs. 1 lakh loan");
    rate = 9.5; // Fixed rate for seniors
}
```

### Auto-ID Generation

```csharp
// File: DB/Utilities/IdGenerator.cs

// Customer ID: MLA00001
var lastId = _context.Customers
    .OrderByDescending(c => c.Custid)
    .Select(c => c.Custid)
    .FirstOrDefault();
int nextNumber = 1;
if (!string.IsNullOrEmpty(lastId))
    nextNumber = int.Parse(lastId.Substring(3)) + 1;
return $"MLA{nextNumber:D5}";

// Employee ID: 2600001
var lastEmp = _context.Employees
    .OrderByDescending(e => e.Empid)
    .Select(e => e.Empid)
    .FirstOrDefault();
int nextEmpNumber = 1;
if (!string.IsNullOrEmpty(lastEmp))
    nextEmpNumber = int.Parse(lastEmp.Substring(2)) + 1;
return $"26{nextEmpNumber:D5}";
```

---

## ?? Security Features

### Password Hashing (SHA256)

```csharp
// File: DB/Utilities/PasswordHelper.cs

public static string HashPassword(string password)
{
    if (string.IsNullOrEmpty(password))
        throw new ArgumentNullException(nameof(password));

    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes); // 44-character hash
    }
}

// Example:
// Input: "Dummy"
// Output: "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg="
```

### Password Verification

```csharp
public static bool VerifyPassword(string password, string storedHash)
{
    if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
        return false;

    string hashOfInput = HashPassword(password);
    return hashOfInput.Equals(storedHash, StringComparison.Ordinal);
}
```

### Backward Compatibility (Login)

```csharp
// File: BankApp.Services/AuthService.cs

public LoginResult ValidateLogin(string username, string password)
{
    var user = _userRepo.GetUserByUsername(username);
    if (user == null) return Error("Invalid username or password");

    bool isPasswordValid = false;
    
    // Try hashed password first (new method)
    if (PasswordHelper.VerifyPassword(password, user.PasswordHash))
        isPasswordValid = true;
    // Fallback to plain text (for migration)
    else if (user.PasswordHash == password)
        isPasswordValid = true;

    if (!isPasswordValid)
        return Error("Invalid username or password");

    return Success("Login successful", user.UserID, user.UserName, 
                   user.Role, user.ReferenceID);
}
```

### Change Password Flow

```csharp
// File: BankApp.Services/AuthService.cs

public LoginResult ChangePassword(string userId, string oldPassword, 
                                   string newPassword, string confirmNewPassword)
{
    // Validate inputs
    if (newPassword != confirmNewPassword)
        return Error("New passwords do not match");
    
    string error = PasswordHelper.ValidatePassword(newPassword);
    if (error != null) return Error(error);

    // Verify old password
    var user = _userRepo.GetUserByUserId(userId);
    if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
        return Error("Current password is incorrect");

    if (oldPassword == newPassword)
        return Error("New password must be different from current password");

    // Update with hashed password
    bool success = _userRepo.UpdatePassword(userId, newPassword);
    return success ? Success("Password changed successfully!") 
                   : Error("Failed to change password");
}
```

### Security Level: ????? (3/5 - Good for Academic)

**What's Secure:**
- ? Password hashing (SHA256)
- ? Session management
- ? Role-based authorization
- ? SQL injection protection (EF parameterized queries)
- ? XSS protection (Razor encoding)

**What's Missing (Production-Grade):**
- ? Salt + bcrypt (using SHA256 instead)
- ? HTTPS enforcement
- ? CSRF tokens
- ? Rate limiting
- ? Password expiry
- ? 2FA
- ? Audit logging

---

## ?? Installation

### Prerequisites

- **Visual Studio 2019/2022** (Community Edition or higher)
- **SQL Server 2019** (Express Edition or higher)
- **SQL Server Management Studio (SSMS)**
- **.NET Framework 4.8**
- **IIS Express** (comes with Visual Studio)

### Step 1: Clone Repository

```bash
git clone https://github.com/MrFranklink/Bank_Destroyer.git
cd Bank_Destroyer
```

### Step 2: Database Setup

1. Open **SQL Server Management Studio**
2. Connect to your SQL Server instance
3. Create a new database:

```sql
CREATE DATABASE Banking_Details;
```

4. Run the database creation scripts (if provided) or let Entity Framework create tables automatically

### Step 3: Update Connection String

Open `Bank_App/Web.config` and update the connection string:

```xml
<connectionStrings>
  <add name="Banking_DetailsEntities" 
       connectionString="metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;
       provider=System.Data.SqlClient;
       provider connection string=&quot;
       data source=YOUR_SERVER_NAME;
       initial catalog=Banking_Details;
       integrated security=True;
       MultipleActiveResultSets=True;
       App=EntityFramework&quot;" 
       providerName="System.Data.EntityClient" />
</connectionStrings>
```

Replace `YOUR_SERVER_NAME` with:
- `(local)\SQLEXPRESS` (if using SQL Server Express)
- `localhost` (if using full SQL Server)
- Your server name

### Step 4: Restore NuGet Packages

Open the solution in Visual Studio and restore NuGet packages:

```
Right-click on Solution ? Restore NuGet Packages
```

Or via Package Manager Console:

```powershell
Update-Package -reinstall
```

### Step 5: Build Solution

```
Build ? Build Solution (Ctrl + Shift + B)
```

### Step 6: Create Initial Manager Account

Run this SQL script in SSMS:

```sql
USE Banking_Details;

-- Insert Manager
INSERT INTO Manager (ManagerId, ManagerName, Pan)
VALUES ('MGR001', 'Admin Manager', 'ADMN0001');

-- Insert UserLogin for Manager
INSERT INTO UserLogin (UserID, UserName, PasswordHash, Role, ReferenceID)
VALUES ('USER0001', 'admin', 'Dummy', 'MANAGER', 'MGR001');
```

### Step 7: Run Application

Press **F5** or click **Start** in Visual Studio

The application will open at: `http://localhost:PORT/Auth/Login`

---

## ?? Configuration

### Web.config Settings

#### Connection String
```xml
<connectionStrings>
  <add name="Banking_DetailsEntities" 
       connectionString="..." 
       providerName="System.Data.EntityClient" />
</connectionStrings>
```

#### Authentication
```xml
<authentication mode="Forms">
  <forms loginUrl="~/Auth/Login" timeout="2880" />
</authentication>
```

#### Session State
```xml
<sessionState mode="InProc" timeout="30" />
```

### App Settings (Optional)

Add custom settings:

```xml
<appSettings>
  <add key="MinimumSavingsBalance" value="1000" />
  <add key="MinimumFDAmount" value="10000" />
  <add key="MinimumLoanAmount" value="10000" />
  <add key="MaxEMIPercentage" value="60" />
</appSettings>
```

---

## ?? Usage

### 1. Login

Navigate to `/Auth/Login`

**Default Credentials:**
- **Username:** `admin`
- **Password:** `Dummy`
- **Role:** Manager

### 2. Register Customer (Manager/Employee)

1. Login as Manager or Employee (DEPT01/DEPT02)
2. Go to **Register Customer** tab
3. Fill in details:
   - Customer Name
   - Date of Birth (must be 18+)
   - PAN Number (format: ABCD1234)
   - Phone Number (optional)
   - Address (optional)
4. Click **Register Customer**
5. Note the generated credentials:
   - **Customer ID:** `MLA00001`
   - **Username:** `johnsmith`
   - **Password:** `Dummy`

### 3. Open Savings Account (Manager/DEPT01)

1. Go to **Open Accounts** ? **Savings** tab
2. Enter:
   - Customer ID (e.g., MLA00001)
   - Initial Deposit (min Rs. 1,000)
3. Click **Open Savings Account**
4. Account ID generated: `SB00001`

### 4. Process Deposit (Manager/DEPT01/Customer)

**As Manager/DEPT01:**
1. Go to **Transactions** ? **Deposit** tab
2. Enter:
   - Savings Account ID (e.g., SB00001)
   - Deposit Amount (min Rs. 100)
3. Click **Process Deposit**

**As Customer:**
1. Login as customer
2. Go to **Deposit/Withdraw** tab
3. Enter deposit amount
4. Click **Deposit Now**

### 5. Change Password (All Users)

1. Click **Change Password** button (top-right corner)
2. Enter:
   - Current Password
   - New Password (min 6 characters)
   - Confirm New Password
3. Click **Change Password**
4. Logout and login with new password

### 6. View Transaction History (Manager/Customer)

**As Manager:**
1. Go to **Manage Accounts** tab
2. Click **View History** on any savings account

**As Customer:**
1. Go to **Transaction History** tab
2. Select your savings account
3. Click **View Transactions**

---

## ?? Screenshots

### Login Page
![Login Page](screenshots/login.png)
*Modern login interface with gradient background*

### Manager Dashboard
![Manager Dashboard](screenshots/manager_dashboard.png)
*Full access dashboard with 7 tabs*

### Employee Dashboard (DEPT01)
![Employee DEPT01](screenshots/employee_dept01.png)
*Blue gradient for Deposit Management*

### Employee Dashboard (DEPT02)
![Employee DEPT02](screenshots/employee_dept02.png)
*Pink gradient for Loan Management*

### Customer Dashboard
![Customer Dashboard](screenshots/customer_dashboard.png)
*Green gradient for customer self-service*

### Change Password
![Change Password](screenshots/change_password.png)
*Role-specific gradient background*

### Account Opening
![Open Account](screenshots/open_account.png)
*Dynamic tabs for different account types*

### Transaction Processing
![Transactions](screenshots/transactions.png)
*Deposit and withdrawal interface*

---

## ?? Project Structure

```
Bank_Destroyer/
??? Bank_App/                           # MVC Web Application
?   ??? Controllers/
?   ?   ??? AuthController.cs          # Login/Logout/Register
?   ?   ??? DashboardController.cs     # All dashboard operations
?   ??? Views/
?   ?   ??? Auth/
?   ?   ?   ??? Login.cshtml
?   ?   ?   ??? Register.cshtml
?   ?   ??? Dashboard/
?   ?   ?   ??? ManagerDashboard.cshtml
?   ?   ?   ??? EmployeeDashboard.cshtml
?   ?   ?   ??? CustomerDashboard.cshtml
?   ?   ?   ??? ChangePassword.cshtml
?   ?   ??? Shared/
?   ?       ??? Template.cshtml
?   ??? Content/                        # CSS, Images
?   ??? Scripts/                        # JavaScript
?   ??? Web.config                      # Configuration
?   ??? packages.config                 # NuGet packages
?
??? BankApp.Services/                   # Business Logic Layer
?   ??? AuthService.cs                  # Authentication + Password
?   ??? CustomerService.cs              # Customer operations
?   ??? EmployeeService.cs              # Employee operations
?   ??? ManagerService.cs               # Manager operations
?   ??? SavingsAccountService.cs        # Savings operations
?   ??? FixedDepositAccountService.cs   # FD operations
?   ??? LoanAccountService.cs           # Loan operations
?   ??? SavingsTransactionService.cs    # Transactions
?   ??? AccountManagementService.cs     # Account views
?
??? DB/                                 # Data Access Layer
?   ??? Utilities/
?   ?   ??? IdGenerator.cs             # Auto-generate IDs
?   ?   ??? PasswordHelper.cs          # SHA256 hashing
?   ??? Repositories/
?   ?   ??? UserLoginRepository.cs     # Auth CRUD
?   ?   ??? CustomerRepository.cs
?   ?   ??? EmployeeRepository.cs
?   ?   ??? ManagerRepository.cs
?   ?   ??? AccountRepository.cs
?   ?   ??? SavingsAccountRepository.cs
?   ?   ??? FixedDepositAccountRepository.cs
?   ?   ??? LoanAccountRepository.cs
?   ?   ??? SavingsTransactionRepository.cs
?   ??? Model1.edmx                     # Entity Framework Model
?   ??? Customer.cs                     # EF Entity
?   ??? Employee.cs
?   ??? Manager.cs
?   ??? UserLogin.cs
?   ??? Account.cs
?   ??? SavingsAccount.cs
?   ??? FixedDepositAccount.cs
?   ??? LoanAccount.cs
?   ??? SavingsTransaction.cs
?
??? DOCS/                               # Documentation
?   ??? Password_Security_Implementation.md
?   ??? Password_Security_Testing_Guide.md
?   ??? PAN_Validation_Guide.md
?   ??? Employee_Dashboard_Implementation.md
?   ??? UserLogin_Fix_Summary.md
?
??? SQL_Scripts/                        # Database scripts
?   ??? Fix_UserLogin_UserID.sql
?   ??? Add_PAN_Uniqueness_Constraints.sql
?
??? screenshots/                        # UI screenshots
?
??? Bank_Destroyer.sln                  # Visual Studio Solution
??? README.md                           # This file
```

---

## ?? API Reference

### Session Variables

```csharp
Session["UserID"]      // e.g., "USER0001"
Session["UserName"]    // e.g., "johnsmith"
Session["Role"]        // "MANAGER", "EMPLOYEE", "CUSTOMER"
Session["ReferenceID"] // "MGR001", "2600001", "MLA00001"
Session["DeptId"]      // "DEPT01", "DEPT02" (employees only)
```

### Key Service Methods

#### AuthService

```csharp
LoginResult ValidateLogin(string username, string password)
LoginResult RegisterUser(string userId, string userName, string password, 
                         string confirmPassword, string role, string referenceId)
LoginResult ChangePassword(string userId, string oldPassword, 
                          string newPassword, string confirmNewPassword)
```

#### CustomerService

```csharp
RegistrationResult RegisterCustomer(string custName, DateTime dob, 
                                    string pan, string address, string phone)
List<CustomerDTO> GetAllCustomers()
int GetCustomerCount()
```

#### AccountService

```csharp
AccountOperationResult OpenSavingsAccount(string customerId, decimal initialDeposit, 
                                         string openedBy, string openedByRole)
AccountOperationResult OpenFixedDepositAccount(string customerId, decimal amount, 
                                               DateTime startDate, int tenureMonths, 
                                               string openedBy, string openedByRole)
AccountOperationResult OpenLoanAccount(string customerId, decimal loanAmount, 
                                       DateTime startDate, int tenureMonths, 
                                       decimal monthlySalary, string openedBy, 
                                       string openedByRole)
```

#### TransactionService

```csharp
TransactionResult Deposit(string accountId, decimal amount)
TransactionResult Withdraw(string accountId, decimal amount)
List<TransactionDTO> GetTransactionHistory(string accountId)
```

---

## ?? Testing

### Manual Testing Checklist

#### Authentication Tests
- [ ] Login with valid credentials (Manager, Employee, Customer)
- [ ] Login with invalid credentials ? Error message
- [ ] Logout ? Session cleared
- [ ] Change password ? Success
- [ ] Login with new password ? Success

#### Manager Tests
- [ ] Register customer ? Credentials displayed
- [ ] Register employee ? Credentials displayed
- [ ] Open savings account ? Account ID generated
- [ ] Open fixed deposit ? Maturity calculated
- [ ] Open loan account ? EMI calculated
- [ ] Process deposit ? Balance updated
- [ ] Process withdrawal ? Balance updated
- [ ] Close account ? Status = CLOSED
- [ ] Delete customer ? Record removed
- [ ] Delete employee ? Record removed
- [ ] View transaction history ? All transactions shown

#### Employee DEPT01 Tests
- [ ] Register customer ? Success
- [ ] Open savings account ? Success
- [ ] Open fixed deposit ? Success
- [ ] Open loan account ? Access denied ?
- [ ] Process deposit ? Success
- [ ] Process withdrawal ? Access denied ?
- [ ] Close savings/FD ? Success
- [ ] Close loan ? Access denied ?

#### Employee DEPT02 Tests
- [ ] Register customer ? Success
- [ ] Open savings account ? Access denied ?
- [ ] Open fixed deposit ? Access denied ?
- [ ] Open loan account ? Success
- [ ] Process deposit ? Access denied ?
- [ ] Close loan ? Success
- [ ] Close savings/FD ? Access denied ?

#### Customer Tests
- [ ] View own profile ? Details shown
- [ ] View own accounts ? All accounts listed
- [ ] Deposit to savings ? Balance updated
- [ ] Withdraw from savings ? Balance updated
- [ ] View transaction history ? Own transactions shown
- [ ] View other customers ? Access denied ?
- [ ] Open account ? Access denied ?

#### Validation Tests
- [ ] PAN uniqueness ? Error if duplicate
- [ ] Age < 18 ? Error
- [ ] Savings initial deposit < Rs. 1,000 ? Error
- [ ] FD amount < Rs. 10,000 ? Error
- [ ] Loan amount < Rs. 10,000 ? Error
- [ ] EMI > 60% of salary ? Error
- [ ] Senior loan > Rs. 1 lakh ? Error
- [ ] Withdrawal leaving < Rs. 1,000 balance ? Error

### Test Credentials

```
Manager:
  Username: admin
  Password: Dummy
  Expected: Full access to all features

Employee DEPT01:
  Username: (check database)
  Password: Dummy
  Expected: Deposit management features only

Employee DEPT02:
  Username: (check database)
  Password: Dummy
  Expected: Loan management features only

Customer:
  Username: (check database)
  Password: Dummy
  Expected: Self-service features only
```

### Database Verification Queries

```sql
-- Check all users
SELECT UserID, UserName, PasswordHash, Role, ReferenceID FROM UserLogin;

-- Check customers
SELECT Custid, Custname, Pan, DOB FROM Customer;

-- Check employees
SELECT Empid, EmployeeName, DeptId, Pan FROM Employee;

-- Check accounts
SELECT AccountID, CustomerID, AccountType, OpenDate, Status FROM Account;

-- Check savings balances
SELECT sa.AccountID, sa.Balance, a.CustomerID, a.Status
FROM SavingsAccount sa
JOIN Account a ON sa.AccountID = a.AccountID;

-- Check transactions
SELECT * FROM SavingsTransaction ORDER BY Transationdate DESC;
```

---

## ?? Known Issues & Solutions

### Issue 1: Cannot Login After Password Hashing ? FIXED

**Problem:** Existing passwords are plain text, new system expects hashed

**Solution:** Added backward compatibility in `AuthService.ValidateLogin()`

```csharp
// Try hashed first, fallback to plain text
if (PasswordHelper.VerifyPassword(password, user.PasswordHash))
    isPasswordValid = true;
else if (user.PasswordHash == password) // Plain text fallback
    isPasswordValid = true;
```

**Manual Fix:** Update all passwords to hashed version:

```sql
UPDATE UserLogin
SET PasswordHash = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg='
WHERE PasswordHash = 'Dummy';
```

### Issue 2: ReferenceID Null ? FIXED

**Problem:** ReferenceID not set during registration

**Solution:** Set `ReferenceID = generated ID` in registration flow

```csharp
// After creating customer
_userLoginRepo.CreateUser(userId, username, "Dummy", "CUSTOMER", custId);
```

### Issue 3: PAN Duplicate Across Tables ? FIXED

**Problem:** Duplicate PANs allowed in Customer, Employee, Manager

**Solution:** Cross-table validation before registration

```csharp
bool panExistsInCustomer = _customerRepo.PANExists(pan);
bool panExistsInEmployee = _employeeRepo.PANExists(pan);
bool panExistsInManager = _managerRepo.PANExists(pan);

if (panExistsInCustomer || panExistsInEmployee || panExistsInManager)
    return Error("PAN already registered");
```

### Issue 4: Department Permissions Not Working ? FIXED

**Problem:** All employees had same permissions

**Solution:** Check `Session["DeptId"]` in controller actions

```csharp
string deptId = Session["DeptId"]?.ToString();
if (role == "EMPLOYEE" && deptId != "DEPT01")
    return Error("Only DEPT01 can open savings accounts");
```

---

## ?? Future Enhancements

### Phase 1: Core Improvements
- [ ] Search and filter functionality (customers, accounts, transactions)
- [ ] Export to Excel/PDF (reports, transaction history)
- [ ] Forgot password with email verification
- [ ] Password strength meter
- [ ] Session timeout warning

### Phase 2: Advanced Features
- [ ] Dashboard analytics with charts (Chart.js)
- [ ] Email/SMS notifications (transaction alerts)
- [ ] Audit logging (user actions tracking)
- [ ] DEPT03 (HR Department) implementation
- [ ] Employee attendance tracking
- [ ] Salary management

### Phase 3: Security Enhancements
- [ ] Implement bcrypt with salt (replace SHA256)
- [ ] Two-Factor Authentication (2FA)
- [ ] HTTPS enforcement
- [ ] CSRF protection
- [ ] Rate limiting on login
- [ ] Password expiry policy

### Phase 4: Performance & Scalability
- [ ] Implement async/await for database operations
- [ ] Add caching (Redis)
- [ ] Implement CQRS pattern
- [ ] Add unit tests (xUnit/NUnit)
- [ ] Integration tests
- [ ] Load testing

### Phase 5: Modernization
- [ ] Migrate to .NET Core/.NET 8
- [ ] Implement Web API + JWT authentication
- [ ] Add Swagger documentation
- [ ] Implement SignalR for real-time updates
- [ ] Progressive Web App (PWA) support
- [ ] Mobile app (Xamarin/MAUI)

---

## ?? Contributing

Contributions are welcome! Please follow these guidelines:

### How to Contribute

1. **Fork the repository**
2. **Create a feature branch:**
   ```bash
   git checkout -b feature/YourFeatureName
   ```
3. **Commit your changes:**
   ```bash
   git commit -m "Add: Your feature description"
   ```
4. **Push to the branch:**
   ```bash
   git push origin feature/YourFeatureName
   ```
5. **Open a Pull Request**

### Coding Standards

- Follow C# coding conventions
- Use meaningful variable/method names
- Add XML comments for public methods
- Write unit tests for new features
- Update documentation (README.md)

### Commit Message Format

```
Type: Short description

Detailed description (if needed)

Types:
- Add: New feature
- Fix: Bug fix
- Update: Update existing feature
- Refactor: Code refactoring
- Docs: Documentation changes
- Style: Code style changes (formatting)
- Test: Add/update tests
```

---

## ?? License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2024 Banking Application

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

## ?? Contact

### Project Maintainer
- **GitHub:** [@MrFranklink](https://github.com/MrFranklink)
- **Repository:** [Bank_Destroyer](https://github.com/MrFranklink/Bank_Destroyer)

### Support
For bug reports, feature requests, or questions:
- **GitHub Issues:** [Create an issue](https://github.com/MrFranklink/Bank_Destroyer/issues)
- **Pull Requests:** [Submit a PR](https://github.com/MrFranklink/Bank_Destroyer/pulls)

---

## ?? Acknowledgments

### Technologies Used
- **ASP.NET MVC** - Microsoft
- **Entity Framework** - Microsoft
- **Bootstrap** - Twitter
- **Bootstrap Icons** - Bootstrap Team
- **SQL Server** - Microsoft

### Inspiration
This project was developed as an **academic final year project** to demonstrate:
- Understanding of ASP.NET MVC architecture
- Entity Framework database-first approach
- Banking domain knowledge
- Role-based access control
- Modern UI/UX design principles

### Special Thanks
- Visual Studio Team for excellent IDE
- Stack Overflow community for troubleshooting help
- Bootstrap team for responsive UI framework
- GitHub Copilot for development assistance

---

## ?? Project Statistics

- **Lines of Code:** ~15,000
- **Files:** 50+
- **Database Tables:** 9
- **Services:** 8
- **Controllers:** 2
- **Views:** 7
- **Repositories:** 9
- **Utilities:** 2
- **Development Time:** ~3 months
- **Complexity Level:** ????? (Beginner/Intermediate)

---

## ?? Academic Use

This project is suitable for:
- ? College/University final year projects
- ? Learning ASP.NET MVC
- ? Understanding Entity Framework
- ? Junior developer portfolio
- ? Interview preparation
- ? Banking domain understanding

**Important Notes:**
- This is an **educational project** and should **NOT** be used for production banking
- Security measures are basic and suitable for academic purposes only
- For production use, implement enterprise-grade security (bcrypt, 2FA, HTTPS, etc.)
- Follow banking regulations and compliance requirements

---

## ?? Version History

### v1.0.0 (Current) - December 2024
- ? Initial release
- ? Complete authentication system
- ? Role-based dashboards (Manager, Employee, Customer)
- ? Account management (Savings, FD, Loan)
- ? Transaction processing (Deposit, Withdraw)
- ? Password hashing (SHA256)
- ? Change password feature
- ? Department-based permissions (DEPT01, DEPT02)
- ? Modern UI with gradients and animations
- ? PAN uniqueness validation
- ? Auto-ID generation
- ? Transaction history
- ? Account closure
- ? Staff management (delete)

### Planned v2.0.0
- [ ] Search and filter functionality
- [ ] Reports and analytics
- [ ] Export to Excel/PDF
- [ ] Email notifications
- [ ] Forgot password
- [ ] Async/await implementation

---

## ?? Quick Links

- [Installation Guide](#-installation)
- [Usage Guide](#-usage)
- [API Reference](#-api-reference)
- [Testing Guide](#-testing)
- [Troubleshooting](#-known-issues--solutions)
- [Contributing Guidelines](#-contributing)
- [Documentation Files](DOCS/)
- [SQL Scripts](SQL_Scripts/)

---

## ?? Notes

### For Students
- Use this project as a reference for your own banking application
- Understand the 3-layer architecture pattern
- Learn role-based access control implementation
- Study Entity Framework database-first approach
- Explore modern UI/UX design with gradients

### For Developers
- This is a beginner/intermediate level project
- Focus on clean code and proper architecture
- Security is basic (suitable for academic purposes)
- Can be extended with advanced features
- Good starting point for learning ASP.NET MVC

### For Evaluators
- Demonstrates understanding of MVC pattern
- Shows proper separation of concerns
- Implements role-based authorization
- Includes password security (hashing)
- Modern and professional UI/UX
- Complete CRUD operations
- Business logic implementation
- Validation rules

---

## ?? Thank You!

Thank you for checking out the **Banking Application** project! If you find it helpful, please:

- ? **Star the repository** on GitHub
- ?? **Fork it** to customize for your needs
- ?? **Share it** with fellow students/developers
- ?? **Report bugs** via GitHub Issues
- ?? **Suggest features** via Pull Requests

**Happy Coding! ??**

---

<div align="center">

**Made with ?? for learning and education**

[? Back to Top](#-banking-application---aspnet-mvc)

</div>
