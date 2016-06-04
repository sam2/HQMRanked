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
        public static List<RankedPlayer> LoggedInPlayers = new List<RankedPlayer>();

        public static void HandleNewLogins(Command cmd)
        {
            if (cmd.Cmd == "join" && cmd.Args.Length > 0)
            {
                RemoveLoggedOutPlayers();
                if (LoginManager.Login(cmd.Sender, cmd.Args[0]))
                {
                    return;
                }
                else
                {
                    LoginManager.CreateNewUser(cmd.Sender.Name, cmd.Args[0]);
                    LoginManager.Login(cmd.Sender, cmd.Args[0]);
                    UserSaveData.SaveUserData();
                }
            }
        }

        public static RankedPlayer IsLoggedIn(int slot)
        {
            foreach(RankedPlayer rp in LoggedInPlayers)
            {
                if (rp.HQMPlayer.Slot == slot)
                    return rp;
            }
            return null;
        }

        static bool Login(Player player, string password)
        {
            UserData u;
            if(UserSaveData.AllUserData.TryGetValue(player.Name, out u))
            {
                RankedPlayer rankedPlayer = new RankedPlayer(player, u);
                if(LoggedInPlayers.Where(x=> x.HQMPlayer.Name == player.Name).Count() > 0)
                {
                    Chat.SendMessage(u.Name + " is already logged in.");
                }
                else if(u.Password == password)
                {
                    Chat.SendMessage(u.Name + " is now logged in.");
                    LoggedInPlayers.Add(rankedPlayer);                    
                } 
                else
                {
                    Chat.SendMessage(u.Name + " - wrong password.");
                }                
                return true;
            }
            return false;
        }

        static bool CreateNewUser(string name, string password)
        {
            if(password == "")
            {
                Chat.SendMessage("invalid password");
                return false;
            }
            UserData u;
            if (UserSaveData.AllUserData.TryGetValue(name, out u))
            {
                Console.WriteLine("User " + u.Name + " already exists.");
                return false;
            }
            u = new UserData(name, password, Moserware.Skills.GameInfo.DefaultGameInfo.DefaultRating);
            UserSaveData.AllUserData[u.Name] = u;
            Chat.SendMessage("New user " + name + " has been created.");
            return true;
        }       

        public static void RemoveLoggedOutPlayers()
        {
            LoggedInPlayers.RemoveAll(player => !player.HQMPlayer.InServer);
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

    public class RankedPlayer
    {
        public Player HQMPlayer;
        public UserData UserData;
        public HQMTeam AssignedTeam = HQMTeam.NoTeam;

        public RankedPlayer(Player p, UserData u)
        {
            HQMPlayer = p;
            UserData = u;
        }
    }


}
