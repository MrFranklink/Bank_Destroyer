using System;
using System.Linq;

namespace DB.Utilities
{
    /// <summary>
    /// Utility class for generating auto-incremented IDs for various entities
    /// </summary>
    public static class IdGenerator
    {
        /// <summary>
        /// Generate next Customer ID in format: MLA + 5 digits (e.g., MLA00001)
        /// </summary>
        public static string GenerateCustomerId()
        {
            using (var context = new Banking_DetailsEntities())
            {
                var lastCustomer = context.Customers
                    .OrderByDescending(c => c.Custid)
                    .FirstOrDefault();

                if (lastCustomer == null)
                {
                    return "MLA00001";
                }

                // Extract numeric part and increment
                string lastId = lastCustomer.Custid;
                if (lastId.StartsWith("MLA") && lastId.Length == 8)
                {
                    int numericPart = int.Parse(lastId.Substring(3));
                    int nextNumber = numericPart + 1;
                    return $"MLA{nextNumber:D5}";
                }

                return "MLA00001";
            }
        }

        /// <summary>
        /// Generate next Employee ID in format: 26 + 5 digits (e.g., 2600001)
        /// </summary>
        public static string GenerateEmployeeId()
        {
            using (var context = new Banking_DetailsEntities())
            {
                var lastEmployee = context.Employees
                    .OrderByDescending(e => e.Empid)
                    .FirstOrDefault();

                if (lastEmployee == null)
                {
                    return "2600001";
                }

                // Extract numeric part and increment
                string lastId = lastEmployee.Empid;
                if (lastId.StartsWith("26") && lastId.Length == 7)
                {
                    int numericPart = int.Parse(lastId.Substring(2));
                    int nextNumber = numericPart + 1;
                    return $"26{nextNumber:D5}";
                }

                return "2600001";
            }
        }

        /// <summary>
        /// Generate next Savings Account ID in format: SB + 5 digits (e.g., SB00001)
        /// Checks both Account and SavingsAccount tables to avoid duplicates
        /// </summary>
        public static string GenerateSavingsAccountId()
        {
            using (var context = new Banking_DetailsEntities())
            {
                // Check both Account table (master) and SavingsAccount table
                var lastFromAccount = context.Accounts
                    .Where(a => a.AccountID.StartsWith("SB"))
                    .OrderByDescending(a => a.AccountID)
                    .Select(a => a.AccountID)
                    .FirstOrDefault();

                var lastFromSavings = context.SavingsAccounts
                    .OrderByDescending(a => a.SBAccountID)
                    .Select(a => a.SBAccountID)
                    .FirstOrDefault();

                // Get the higher ID from both tables
                string lastId = null;
                if (lastFromAccount != null && lastFromSavings != null)
                {
                    lastId = string.Compare(lastFromAccount, lastFromSavings) > 0 ? lastFromAccount : lastFromSavings;
                }
                else
                {
                    lastId = lastFromAccount ?? lastFromSavings;
                }

                if (lastId == null)
                {
                    return "SB00001";
                }

                if (lastId.StartsWith("SB") && lastId.Length == 7)
                {
                    int numericPart = int.Parse(lastId.Substring(2));
                    int nextNumber = numericPart + 1;
                    return $"SB{nextNumber:D5}";
                }

                return "SB00001";
            }
        }

        /// <summary>
        /// Generate next Fixed Deposit Account ID in format: FD + 5 digits (e.g., FD00001)
        /// Checks both Account and FixedDepositAccount tables to avoid duplicates
        /// </summary>
        public static string GenerateFixedDepositAccountId()
        {
            using (var context = new Banking_DetailsEntities())
            {
                var lastFromAccount = context.Accounts
                    .Where(a => a.AccountID.StartsWith("FD"))
                    .OrderByDescending(a => a.AccountID)
                    .Select(a => a.AccountID)
                    .FirstOrDefault();

                var lastFromFD = context.FixedDepositAccounts
                    .OrderByDescending(a => a.FDAccountID)
                    .Select(a => a.FDAccountID)
                    .FirstOrDefault();

                string lastId = null;
                if (lastFromAccount != null && lastFromFD != null)
                {
                    lastId = string.Compare(lastFromAccount, lastFromFD) > 0 ? lastFromAccount : lastFromFD;
                }
                else
                {
                    lastId = lastFromAccount ?? lastFromFD;
                }

                if (lastId == null)
                {
                    return "FD00001";
                }

                if (lastId.StartsWith("FD") && lastId.Length == 7)
                {
                    int numericPart = int.Parse(lastId.Substring(2));
                    int nextNumber = numericPart + 1;
                    return $"FD{nextNumber:D5}";
                }

                return "FD00001";
            }
        }

        /// <summary>
        /// Generate next Loan Account ID in format: LN + 5 digits (e.g., LN00001)
        /// Checks both Account and LoanAccount tables to avoid duplicates
        /// </summary>
        public static string GenerateLoanAccountId()
        {
            using (var context = new Banking_DetailsEntities())
            {
                var lastFromAccount = context.Accounts
                    .Where(a => a.AccountID.StartsWith("LN"))
                    .OrderByDescending(a => a.AccountID)
                    .Select(a => a.AccountID)
                    .FirstOrDefault();

                var lastFromLoan = context.LoanAccounts
                    .OrderByDescending(a => a.Ln_accountid)
                    .Select(a => a.Ln_accountid)
                    .FirstOrDefault();

                string lastId = null;
                if (lastFromAccount != null && lastFromLoan != null)
                {
                    lastId = string.Compare(lastFromAccount, lastFromLoan) > 0 ? lastFromAccount : lastFromLoan;
                }
                else
                {
                    lastId = lastFromAccount ?? lastFromLoan;
                }

                if (lastId == null)
                {
                    return "LN00001";
                }

                if (lastId.StartsWith("LN") && lastId.Length == 7)
                {
                    int numericPart = int.Parse(lastId.Substring(2));
                    int nextNumber = numericPart + 1;
                    return $"LN{nextNumber:D5}";
                }

                return "LN00001";
            }
        }

        /// <summary>
        /// Generate username from first name + 4 random digits (e.g., john1234)
        /// </summary>
        public static string GenerateUsername(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                firstName = "user";
            }

            // Clean the first name (remove spaces, special chars, convert to lowercase)
            string cleanName = new string(firstName.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLower();
            
            if (string.IsNullOrEmpty(cleanName))
            {
                cleanName = "user";
            }

            // Generate 4 random digits
            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);

            string username = $"{cleanName}{randomNumber}";

            // Ensure uniqueness
            using (var context = new Banking_DetailsEntities())
            {
                while (context.UserLogins.Any(u => u.UserName == username))
                {
                    randomNumber = random.Next(1000, 9999);
                    username = $"{cleanName}{randomNumber}";
                }
            }

            return username;
        }

        /// <summary>
        /// Validate PAN format: 5 letters + 4 digits + 1 letter (e.g., ABCDE1234F)
        /// Real Indian PAN format:
        /// - First 3 chars: Alphabetic series (AAA to ZZZ)
        /// - 4th char: Status code (P=Person, C=Company, H=HUF, F=Firm, etc.)
        /// - 5th char: First letter of surname/name
        /// - 6th-9th chars: 4 sequential digits (0000-9999)
        /// - 10th char: Alphabetic check digit
        /// </summary>
        public static bool ValidatePanFormat(string pan)
        {
            if (string.IsNullOrWhiteSpace(pan) || pan.Length != 10)
            {
                return false;
            }

            // Convert to uppercase for validation
            pan = pan.ToUpper();

            // First 5 should be letters
            string firstFive = pan.Substring(0, 5);
            if (!firstFive.All(char.IsLetter))
            {
                return false;
            }

            // Next 4 should be digits (6th to 9th position)
            string fourDigits = pan.Substring(5, 4);
            if (!fourDigits.All(char.IsDigit))
            {
                return false;
            }

            // Last character (10th) should be a letter
            char lastChar = pan[9];
            if (!char.IsLetter(lastChar))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate age from date of birth
        /// </summary>
        public static int CalculateAge(DateTime dateOfBirth)
        {
            int age = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now < dateOfBirth.AddYears(age))
            {
                age--;
            }
            return age;
        }

        /// <summary>
        /// Check if person is senior citizen (age >= 60)
        /// </summary>
        public static bool IsSeniorCitizen(DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth) >= 60;
        }

        /// <summary>
        /// Generate unique UserID for UserLogin table
        /// Format: USR + 5 digits (e.g., USR00001, USR00002)
        /// This is different from ReferenceID (Customer/Employee/Manager ID)
        /// </summary>
        public static string GenerateUserId()
        {
            using (var context = new Banking_DetailsEntities())
            {
                var lastUser = context.UserLogins
                    .Where(u => u.UserID.StartsWith("USR"))
                    .OrderByDescending(u => u.UserID)
                    .FirstOrDefault();

                if (lastUser == null)
                {
                    return "USR00001";
                }

                string lastId = lastUser.UserID;
                if (lastId.StartsWith("USR") && lastId.Length == 8)
                {
                    int numericPart = int.Parse(lastId.Substring(3));
                    int nextNumber = numericPart + 1;
                    return $"USR{nextNumber:D5}";
                }

                return "USR00001";
            }
        }
    }
}
