using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moserware.Skills;

namespace HQMRanked
{
    class LoginManager
    {
        public static Dictionary<string, User> Users;

        public static List<User> LoggedInUsers = new List<User>();

        public static bool Login(string name, string password)
        {
            User u;
            if(Users.TryGetValue(name, out u) && password == u.Password)
            {
                LoggedInUsers.Add(u);
                return true;
            }
            return false;
        }

        public static bool CreateNewUser(string name, string password)
        {
            User u;
            if (Users.TryGetValue(name, out u))
            {
                Console.WriteLine("User " + u.Name + " already exists.");
                return false;
            }    
            u = new User(name, password, GameInfo.DefaultGameInfo.DefaultRating.Mean, GameInfo.DefaultGameInfo.DefaultRating.StandardDeviation);
            Users[u.Name] = u;
            return true;
        }       
    }

    
}
