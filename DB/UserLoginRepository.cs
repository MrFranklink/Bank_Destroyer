using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB
{
    public class UserLoginRepository
    {
        public UserLogin GetUserByUsername(string username)
        {
            using (var context = new Banking_DetailsEntities())
            {
                return context.UserLogins
                    .FirstOrDefault(u => u.UserName == username);
            }
        }

        public bool UsernameExists(string username)
        {
            using (var context = new Banking_DetailsEntities())
            {
                return context.UserLogins.Any(u => u.UserName == username);
            }
        }

        public bool UserIdExists(string userId)
        {
            using (var context = new Banking_DetailsEntities())
            {
                return context.UserLogins.Any(u => u.UserID == userId);
            }
        }

        public bool CreateUser(string userId, string userName, string password, string role, string referenceId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var newUser = new UserLogin
                    {
                        UserID = userId,
                        UserName = userName,
                        PasswordHash = password, // In production, hash this password! //Remeber This
                        Role = role,
                        ReferenceID = referenceId
                    };

                    context.UserLogins.Add(newUser);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Database.Exists();
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
