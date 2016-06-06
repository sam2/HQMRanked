using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HQMRanked
{
    public class UserSaveData
    {
        const string PATH = "UserData.hqm";
        public static Dictionary<string, UserData> AllUserData = new Dictionary<string,UserData>();

        public static Dictionary<string, UserData> ReadUserData()
        {
            Dictionary<string, UserData> users = new Dictionary<string, UserData>();
            
            if (!System.IO.File.Exists(PATH))
            {
                Console.WriteLine("User data not found.");
                SaveUserData();
                return users;
            }                

            string[] raw = System.IO.File.ReadAllLines(PATH);
            foreach(string s in raw)
            {
                string[] user = s.Split('~');
                if (user.Length < 3)
                    continue;
                UserData u = new UserData(user[0], user[1], new Moserware.Skills.Rating(double.Parse(user[2]), double.Parse(user[3])), int.Parse(user[4]), int.Parse(user[5]), int.Parse(user[6]), int.Parse(user[7]));
                users[u.Name] = u;
            }
            return users;
        }

        public static void SaveUserData()
        {
            string[] raw = new string[UserSaveData.AllUserData.Count];

            int i = 0;
            foreach (KeyValuePair<string, UserData> u in UserSaveData.AllUserData)
            {
                UserData d = u.Value;
                raw[i] = d.Name + "~" + d.Password + "~" + u.Value.Rating.Mean + "~" + u.Value.Rating.StandardDeviation + "~"+ d.GamesPlayed + "~"+d.Wins+"~"+d.Goals + "~"+ d.Assists;
                i++;
            }

            System.IO.File.WriteAllLines(PATH, raw);
        }
    }

    public class UserData
    {
        public string Name;
        public string Password;
        public int GamesPlayed;
        public int Wins;
        public int Goals;
        public int Assists;
        public Moserware.Skills.Rating Rating;

        public UserData(string name, string pw, Moserware.Skills.Rating r, int gamesPlayed = 0, int wins = 0, int goals = 0, int assists = 0)
        {
            Name = name;
            Password = pw;
            Rating = r;
            GamesPlayed = gamesPlayed;
            Wins = wins;
            Goals = goals;
            Assists = assists;
        }
    }

   

    
}
