using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQMRanked
{
    public class UserData
    {
        const string PATH = "UserData.hqm";

        public static Dictionary<string, User> ReadUserData()
        {
            Dictionary<string, User> users = new Dictionary<string, User>();
            
            if (!System.IO.File.Exists(PATH))
            {
                Console.WriteLine("User data not found.");
                SaveUserData(users);
                return users;
            }                

            string[] raw = System.IO.File.ReadAllLines(PATH);
            foreach(string s in raw)
            {
                string[] user = s.Split('~');
                User u = new User(user[0], user[1], double.Parse(user[2]), double.Parse(user[3]));
                users[u.Name] = u;
            }
            return users;
        }

        public static void SaveUserData(Dictionary<string, User> users)
        {
            string[] raw = new string[users.Count];

            int i = 0;
            foreach(KeyValuePair<string, User> u in users)
            {
                raw[i] = u.Value.Name + "~" + u.Value.Password + "~" + u.Value.RatingMean + "~" + u.Value.RatingSD;
            }

            System.IO.File.WriteAllLines(PATH, raw);
        }
    }

    public class User
    {
        public string Name;
        public string Password;
        public double RatingMean;
        public double RatingSD;

        public User(string name, string pw, double ratingmean, double ratingsd)
        {
            Name = name;
            Password = pw;
            RatingMean = ratingmean;
            RatingSD = ratingsd;
        }
    }

    
}
