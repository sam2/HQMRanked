using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQMEditorDedicated;

namespace HQMRanked
{
    class LoginManager
    {
        public static Dictionary<string, User> AllUsers;

        public static List<User> LoggedInUsers = new List<User>();

        public static bool Login(string name, string password)
        {
            User u;
            if(AllUsers.TryGetValue(name, out u))
            {
                if(LoggedInUsers.Contains(u))
                {
                    Chat.SendMessage(u.Name + " is already logged in.");
                }
                else if(u.Password == password)
                {
                    Chat.SendMessage(u.Name + " is now logged in.");
                    LoggedInUsers.Add(u);                    
                } 
                else
                {
                    Chat.SendMessage(u.Name + "- wrong Password.");
                }                
                return true;
            }
            return false;
        }

        public static bool CreateNewUser(string name, string password)
        {
            if(password == "")
            {
                Chat.SendMessage("invalid password");
            }
            User u;
            if (AllUsers.TryGetValue(name, out u))
            {
                Console.WriteLine("User " + u.Name + " already exists.");
                return false;
            }
            u = new User(name, password, Moserware.Skills.GameInfo.DefaultGameInfo.DefaultRating);
            AllUsers[u.Name] = u;
            Chat.SendMessage("New user " + name + " has been created.");
            return true;
        }       
    }

    public class User
    {
        public string Name;
        public string Password;
        public Moserware.Skills.Rating Rating;

        public User(string name, string pw, Moserware.Skills.Rating r)
        {
            Name = name;
            Password = pw;
            Rating = r;
        }
    }


}
