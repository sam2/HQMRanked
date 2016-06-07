using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HQMEditorDedicated;

namespace HQMRanked
{
    public class RankedGame
    {
        public bool InProgress = false;

        public bool StartingGame
        {
            get;
            private set;
        }

        List<string> RedTeam = new List<string>();
        List<string> BlueTeam = new List<string>();

        RankedGameReport LastGameReport;

        IEnumerable<IDictionary<string, Moserware.Skills.Rating>> TrueSkillTeamModel;
        
        System.Timers.Timer _timer;

        public void StartGameTimer()
        {
            _timer = new System.Timers.Timer(Util.GAME_START_TIMER*1000);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
            _timer.AutoReset = false;
            _timer.Enabled = true;
            StartingGame = true;
        }

        void TimerElapsed(object sender, EventArgs e)
        {
            if (LoginManager.LoggedInPlayers.Count < Util.MIN_PLAYER_COUNT)
            {
                Chat.SendMessage("Not enough players. Aborting game...");
            }
            else
            {
                StartGame();
            }
            _timer.Enabled = false;
            StartingGame = false;
        }

        public void StartGame()
        {
            SetPlayedLastGame();
            CreateTeams();
            TrueSkillTeamModel = RatingCalculator.BuildTeamModel(RedTeam, BlueTeam);
            Chat.SendMessage("Game Starting with match quality " + Math.Round(RatingCalculator.CalculateMatchQuality(TrueSkillTeamModel), 2));            
            Tools.ResumeGame();
            InProgress = true;
        }

        public void EndGame(bool record)
        {
            Chat.SendMessage("Game over. Recording stats...");    
            LastGameReport = new RankedGameReport(RedTeam, BlueTeam, TrueSkillTeamModel);  
            if(record)
            {                          
                RedditReporter.Instance.PostGameResult(LastGameReport);
                SavePlayerStats(LastGameReport);
                RedditReporter.Instance.UpdateRatings();
            }
            
            Chat.SendMessage("Stats Recorded. Check reddit.com/r/hqmgames for results");
            ClearTeams();
            LoginManager.LoggedInPlayers = new List<RankedPlayer>();
            Chat.SendMessage("---------------------------------------------------");
            Chat.SendMessage("---All players have been logged out.---");
            Chat.SendMessage("---------------------------------------------------");
            //GameInfo.IsGameOver = true;
            Tools.PauseGame();
            InProgress = false;
        }


        public void RemoveTrespassers()
        {
            while(true)
            {
                if (!InProgress && !StartingGame) continue;

                int maxPlayers = ServerInfo.MaxPlayerCount;
                for (int i = 0; i < maxPlayers; i++)
                {
                    byte[] playerMemory = MemoryEditor.ReadBytes(Util.PLAYER_LIST_ADDRESS + i * Util.PLAYER_STRUCT_SIZE, Util.PLAYER_STRUCT_SIZE); //read player struct
                    if (playerMemory[0] == 1)//in server
                    {
                        HQMTeam t = (HQMTeam)playerMemory[Util.TEAM_OFFSET];

                        IEnumerable<byte> namebytes = playerMemory.Skip(0x14).Take(0x18);
                        string name = Encoding.ASCII.GetString(namebytes.ToArray()).Split('\0')[0]; 
                      
                        if (t != HQMTeam.NoTeam) //if on the ice
                        {
                            bool onRightTeam = ((t == HQMTeam.Blue && BlueTeam.Contains(name) || t == HQMTeam.Red && RedTeam.Contains(name)) && LoginManager.IsLoggedIn(name) != null);                          
                            if(!onRightTeam) MemoryEditor.WriteInt(32, Util.PLAYER_LIST_ADDRESS + i * Util.PLAYER_STRUCT_SIZE + Util.LEG_STATE_OFFSET);                                
                        }
                    }
                }
                Thread.Sleep(Util.TRESSPASS_REMOVER_SLEEP);
            }            
        }



        public void CreateTeams()
        {
            //give prio to people who didn't play last game
            List<RankedPlayer> players = LoginManager.LoggedInPlayers.Where(x => !x.PlayedLastGame).ToList();
            List<RankedPlayer> others = LoginManager.LoggedInPlayers.Except(players).ToList();
            Random r = new Random();
            while(players.Count < Math.Min(10, LoginManager.LoggedInPlayers.Count))
            {
                RankedPlayer newPlayer = others[r.Next(others.Count)];                
                players.Add(newPlayer);
                others.Remove(newPlayer);
            }


            
            List<RankedPlayer> SortedRankedPlayers = players.OrderByDescending(x => x.UserData.Rating.ConservativeRating).ToList();
            /*split up goalies
            List<RankedPlayer> goalies = SortedRankedPlayers.Where(x => x.PlayerStruct.Role == HQMRole.G).ToList();
            if(goalies.Count >= 2)
            {
                RedTeam.Add(goalies[0].Name);
                goalies[0].AssignedTeam = HQMTeam.Red;            
                BlueTeam.Add(goalies[1].Name);
                goalies[1].AssignedTeam = HQMTeam.Blue;
                SortedRankedPlayers.Remove(goalies[0]);
                SortedRankedPlayers.Remove(goalies[1]);
            }*/
            

            double half_max = Math.Ceiling((double)SortedRankedPlayers.Count() / 2);

            for(int i = 0; i < SortedRankedPlayers.Count; i++)
            {
                RankedPlayer p = SortedRankedPlayers[i];
                if (TotalRating(RedTeam) < TotalRating(BlueTeam) && RedTeam.Count < half_max)
                {
                    RedTeam.Add(p.Name);
                    p.AssignedTeam = HQMTeam.Red;                    
                }
                else if (BlueTeam.Count < half_max)
                {
                    BlueTeam.Add(p.Name);
                    p.AssignedTeam = HQMTeam.Blue;                
                }                    
                else
                {
                    RedTeam.Add(p.Name);
                    p.AssignedTeam = HQMTeam.Red;                 
                }
            }

            PrintTeams();

            //auto join
            while (players.Where(p => p.PlayerStruct.Team == HQMTeam.NoTeam).Count() > 0)
            {
                foreach (RankedPlayer p in players.Where(p => p.PlayerStruct.Team == HQMTeam.NoTeam))
                {
                    p.PlayerStruct.LockoutTime = 0;
                    if (p.AssignedTeam == HQMTeam.Red)
                    {
                        p.PlayerStruct.LegState = 4;
                    }
                    else if (p.AssignedTeam == HQMTeam.Blue)
                        p.PlayerStruct.LegState = 8;
                }
            }
        }

        void ClearTeams()
        {
            foreach(RankedPlayer p in LoginManager.LoggedInPlayers)
            {
                p.AssignedTeam = HQMTeam.NoTeam;               
            }
            RedTeam = new List<string>();
            BlueTeam = new List<string>();
        }

        double TotalRating(List<string> list)
        {
            double result = 0;
            foreach(string p in list)
            {
                result += UserSaveData.AllUserData[p].Rating.ConservativeRating;
            }
            return result;
        }      

        public void PrintTeams()
        {
            string red = "Red: ";
            foreach (string p in RedTeam)
            {
                red += p + " ";
            }

            string blue = "Blue: ";
            foreach (string p in BlueTeam)
            {
                blue += p + " ";
            }
            Chat.SendMessage(red);
            Chat.SendMessage(blue);
        }

        private void SavePlayerStats(RankedGameReport report)
        {
            foreach(RankedGameReport.PlayerStatLine statline in report.PlayerStats)
            {
                UserData u;
                if(UserSaveData.AllUserData.TryGetValue(statline.Name, out u))
                {
                    u.Goals += statline.Goals;
                    u.Assists += statline.Assists;
                    u.GamesPlayed++;
                    if(statline.Team == report.Winner)
                    {
                        u.Wins++;
                    }
                    u.Rating = report.NewRatings[statline.Name];
                }
            }
            UserSaveData.SaveUserData();
        }

        private void SetPlayedLastGame()
        {
            if (LastGameReport != null)
            {
                List<string> names = new List<string>();
                foreach(RankedGameReport.PlayerStatLine sl in LastGameReport.PlayerStats)
                {
                    names.Add(sl.Name);
                }

                foreach(RankedPlayer rp in LoginManager.LoggedInPlayers)
                {
                    rp.PlayedLastGame = names.Contains(rp.Name);
                }
            }
            
        }
    }
}
