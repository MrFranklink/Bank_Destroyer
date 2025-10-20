using System;
using System.Collections.Generic;
using System.Linq;

namespace DB
{
    public class ManagerRepository
    {
        public bool CreateManager(string managerId, string managerName, string pan)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    // Check if manager already exists
                    if (context.Managers.Any(m => m.ManagerID == managerId))
                    {
                        return false;
                    }

                    var newManager = new Manager
                    {
                        ManagerID = managerId,
                        ManagerName = managerName,
                        PAN = pan
                    };

                    context.Managers.Add(newManager);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<Manager> GetAllManagers()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.ToList();
                }
            }
            catch
            {
                return new List<Manager>();
            }
        }

        public Manager GetManagerById(string managerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.FirstOrDefault(m => m.ManagerID == managerId);
                }
            }
            catch
            {
                return null;
            }
        }

        public bool ManagerExists(string managerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.Any(m => m.ManagerID == managerId);
                }
            }
            catch
            {
                return false;
            }
        }

        public int GetManagerCount()
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    return context.Managers.Count();
                }
            }
            catch
            {
                return 0;
            }
        }

        public bool DeleteManager(string managerId)
        {
            try
            {
                using (var context = new Banking_DetailsEntities())
                {
                    var manager = context.Managers.Find(managerId);
                    if (manager == null)
                    {
                        return false;
                    }

                    context.Managers.Remove(manager);
                    context.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
