using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB;
using DB.Utilities;

namespace BankApp.Services
{
    public class AuthService
    {
        private readonly UserLoginRepository _userRepo;
        private static readonly string[] ValidRoles = { "CUSTOMER", "EMPLOYEE", "MANAGER" };

        public AuthService()
        {
            _userRepo = new UserLoginRepository();
        }

        public LoginResult ValidateLogin(string username, string password)
        {
            var validationRules = new List<Func<LoginResult>>
            {
                () => string.IsNullOrWhiteSpace(username) ? Error("Username is required") : null,
                () => string.IsNullOrWhiteSpace(password) ? Error("Password is required") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            var user = _userRepo.GetUserByUsername(username);
            if (user == null) return Error("Invalid username or password");

            // BACKWARD COMPATIBILITY: Check if password is plain text or hashed
            bool isPasswordValid = false;
            
            // Try hashed password first
            if (PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                isPasswordValid = true;
            }
            // Fallback: Try plain text comparison (for old passwords)
            else if (user.PasswordHash == password)
            {
                isPasswordValid = true;
                
                // OPTIONAL: Auto-upgrade to hashed password on first login
                // Uncomment the line below to automatically hash old passwords
                // _userRepo.UpdatePassword(user.UserID, password);
            }

            if (!isPasswordValid)
                return Error("Invalid username or password");

            return Success("Login successful", user.UserID, user.UserName, user.Role, user.ReferenceID);
        }

        public LoginResult RegisterUser(string userId, string userName, string password, string confirmPassword, string role, string referenceId)
        {
            var validationRules = new List<Func<LoginResult>>
            {
                () => string.IsNullOrWhiteSpace(userId) ? Error("User ID is required") : null,
                () => string.IsNullOrWhiteSpace(userName) ? Error("Username is required") : null,
                () => string.IsNullOrWhiteSpace(password) ? Error("Password is required") : null,
                () => string.IsNullOrWhiteSpace(role) ? Error("Role is required") : null,
                () => password != confirmPassword ? Error("Passwords do not match") : null,
                () => password?.Length < 6 ? Error("Password must be at least 6 characters") : null,
                () => !ValidRoles.Contains(role?.ToUpper()) ? Error("Invalid role selected") : null,
                () => _userRepo.UsernameExists(userName) ? Error("Username already exists") : null,
                () => _userRepo.UserIdExists(userId) ? Error("User ID already exists") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            // Password will be hashed in repository
            bool success = _userRepo.CreateUser(userId, userName, password, role, referenceId);
            return success ? Success("Registration successful! Please login.") : Error("Registration failed. Please try again.");
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public LoginResult ChangePassword(string userId, string oldPassword, string newPassword, string confirmNewPassword)
        {
            var validationRules = new List<Func<LoginResult>>
            {
                () => string.IsNullOrWhiteSpace(oldPassword) ? Error("Current password is required") : null,
                () => string.IsNullOrWhiteSpace(newPassword) ? Error("New password is required") : null,
                () => string.IsNullOrWhiteSpace(confirmNewPassword) ? Error("Please confirm new password") : null,
                () => newPassword != confirmNewPassword ? Error("New passwords do not match") : null
            };

            var validationError = validationRules.Select(rule => rule()).FirstOrDefault(result => result != null);
            if (validationError != null) return validationError;

            // Validate new password strength
            string passwordError = PasswordHelper.ValidatePassword(newPassword);
            if (passwordError != null)
                return Error(passwordError);

            // Get user and verify old password
            var user = _userRepo.GetUserByUserId(userId);
            if (user == null)
                return Error("User not found");

            if (!PasswordHelper.VerifyPassword(oldPassword, user.PasswordHash))
                return Error("Current password is incorrect");

            if (oldPassword == newPassword)
                return Error("New password must be different from current password");

            // Update password
            bool success = _userRepo.UpdatePassword(userId, newPassword);
            return success ? Success("Password changed successfully!") : Error("Failed to change password. Please try again.");
        }

        private LoginResult Error(string message) => new LoginResult { IsSuccess = false, Message = message };
        
        private LoginResult Success(string message, string userId = null, string userName = null, string role = null, string referenceId = null) => 
            new LoginResult 
            { 
                IsSuccess = true, 
                Message = message, 
                UserID = userId, 
                UserName = userName, 
                Role = role, 
                ReferenceID = referenceId 
            };
    }

    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string ReferenceID { get; set; }
    }
}
