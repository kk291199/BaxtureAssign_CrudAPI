using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Assignment.UserRepository
{
    public class ClientDetailsRepo : IDisposable
    {
        private UserMgntEntities db = new UserMgntEntities();

        public ClientDetail ValidateClient(string clientId, string clientSecret)
        {
            return db.ClientDetails.FirstOrDefault(user => user.ClientId == clientId && user.ClientSecret == clientSecret);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}