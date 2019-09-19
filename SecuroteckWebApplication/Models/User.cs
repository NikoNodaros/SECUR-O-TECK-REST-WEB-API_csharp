using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.ObjectModel;
using System.Web.Http;
using static SecuroteckWebApplication.Models.User;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecuroteckWebApplication.Models
{
    public class User
    {
        public enum Roles
        {
            User,
            Admin
        }
        #region Task2
        [Key]
        public string ApiKey { get; set; }
        public string UserName { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public Roles Role { get; set; }
        public User()
        {
            Logs = new Collection<Log>();
        }
        #endregion
    }
    #region Task13?
    // TODO: You may find it useful to add code here for Logging
    public class Log
    {
        [Key]
        public string LogID { get; set; }
        public string LogString { get; set; }
        public DateTime LogDateTime { get; set; }
        public Log(string pLine)
        {
            LogID = Guid.NewGuid().ToString();
            LogString = pLine;
            LogDateTime = DateTime.Now;
        }
        public Log() { }
    }
    public class LogArchive
    {
        [Key]
        public string LogArchiveID { get; set; }
        public string User_ApiKey { get; set; }
        public string LogArchiveString { get; set; }
        public DateTime LogDateTime { get; set; }
        public DateTime ArchivedDate { get; set; }
        public LogArchive(string pGuid, string pApiKey, string pLine, DateTime pLogDateTime)
        {
            LogArchiveID = pGuid;
            User_ApiKey = pApiKey;
            LogArchiveString = pLine;
            LogDateTime = pLogDateTime;
            ArchivedDate = DateTime.Now;
        }
        public LogArchive() { }
    }
    #endregion

    public class UserDatabaseAccess
    {
        #region Task3 
        // TODO: Make methods which allow us to read from/write to the database 
        
        public static User CreateUser(string pUserName)
        {
            using (var dB = new UserContext())
            {
                IQueryable<User> uQuery = from user in dB.Users where user.Role == Roles.Admin select user;
                var newRole = Roles.User;
                if (!uQuery.Any()) newRole = Roles.Admin;

                var virginUser = new User()
                {
                    UserName = pUserName,
                    ApiKey = Guid.NewGuid().ToString(),
                    Role = newRole
                };
                dB.Users.Add(virginUser);
                dB.SaveChanges();
                return virginUser;
            }
        }
        public static bool UserExists(Guid pApiKey, out User pUser)
        {
            using (var dB = new UserContext())
            {
                return (pUser = dB.Users.SingleOrDefault(c => c.ApiKey == pApiKey.ToString())) == null ? false : true;
            }
        }
        public static bool UserExists(string pUserName)
        {
            using (var dB = new UserContext())
            {
                IQueryable<User> userQuery = from user in dB.Users where user.UserName == pUserName select user;
                foreach (User user in userQuery)
                {
                    return true;
                }
                return false;
            }
        }
        public static bool UserExists(string pApiKey, string pUserName)
        {
            using (var dB = new UserContext())
            {
                IQueryable<User> userQuery = from user in dB.Users where user.ApiKey == pApiKey select user;
                foreach (User user in userQuery)
                {
                    if (user.UserName == pUserName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ChangeRole(string pUserName, Roles pNewRole)
        {
            try
            {
                using (var dB = new UserContext())
                {
                    IQueryable<User> userQuery = from user in dB.Users where user.UserName == pUserName select user;
                    foreach (User user in userQuery)
                    {
                        user.Role = pNewRole;
                    }
                    dB.SaveChanges();
                    return true;
                }
            } catch { return false; }
        }
        public static void DeleteUser(string pApiKey)
        {
            using (var dB = new UserContext())
            {
                User user = dB.Users.SingleOrDefault(c => c.ApiKey == pApiKey.ToString());

                int count = user.Logs.Count();
                for (int i = 0; i < count; i++)
                {
                    Log log = user.Logs.ElementAt(i);
                    dB.LogArchives.Add(new LogArchive(log.LogID, user.ApiKey, log.LogString, log.LogDateTime));
                    dB.Logs.Remove(log);
                    count--;
                    i = -1;
                }
                dB.Users.Remove(user);
                dB.SaveChanges();
            }
        }
        public static void Log(string pLogDescription, string pApiKey)
        {
            using (var dB = new UserContext())
            {
                User user = dB.Users.SingleOrDefault(c => c.ApiKey == pApiKey.ToString());

                Log log = new Log(pLogDescription);
                dB.Logs.Add(log);
                user.Logs.Add(log);
                dB.SaveChanges();
            }
        }
        #endregion
    }
}