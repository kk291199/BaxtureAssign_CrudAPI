using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment.UserRepository
{
    public class UserRepo:IDisposable
    {
        private UserMgntEntities db = new UserMgntEntities();

        public void Dispose()
        {
            db.Dispose();
        }

        public  User IsValidUser(string username, string password)
        {
            var user = db.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            return user;
        }
    }
}