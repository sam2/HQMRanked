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

        public bool StartingGame = false;

        public bool IsMercy = false;

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
            GameInfo.IntermissionTime = 0;
            GameInfo.IsGameOver = true;
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
            IsMercy = false;
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
                try
                {
                    RedditReporter.Instance.PostGameResult(LastGameReport);
                }   
                catch(Exception ex)
                {
                    Chat.SendMessage("Could not post game result: " + ex.Message);
                }
                         
                
                SavePlayerStats(LastGameReport);
                try
                {
                    RedditReporter.Instance.UpdateRatings();
                    Chat.SendMessage("Stats Recorded. Check reddit.com/r/hqmgames for results");
                }
                catch (Exception ex)
                {
                    Chat.SendMessage("Could not update ratings post: " + ex.Message);
                }
                
            }           
            
            ClearTeams();
            LoginManager.LoggedInPlayers = new List<RankedPlayer>();
            Chat.SendMessage("---------------------------------------------------");
            Chat.SendMessage("   All players have been logged out.");
            Chat.SendMessage("  Please relog to join the next game.");
            Chat.SendMessage("---------------------------------------------------");
            GameInfo.IsGameOver = true;
            InProgress = false;
        }      

        public void RemoveTrespassers()
        {           
            while(true)
            {
                if (InProgress || StartingGame)
                {
                    byte[] playerList = MemoryEditor.ReadBytes(Util.PLAYER_LIST_ADDRESS, Util.MAX_PLAYERS * Util.PLAYER_STRUCT_SIZE);
                    for (int i = 0; i < Util.MAX_PLAYERS; i++)
                    {
                        if (playerList[i * Util.PLAYER_STRUCT_SIZE] == 1)//in server
                        {
                            HQMTeam t = (HQMTeam)playerList[i * Util.PLAYER_STRUCT_SIZE + Util.TEAM_OFFSET];

                            IEnumerable<byte> namebytes = playerList.Skip(i * Util.PLAYER_STRUCT_SIZE + 0x14).Take(0x18);
                            string name = Encoding.ASCII.GetString(namebytes.ToArray()).Split('\0')[0];

                            if (t != HQMTeam.NoTeam && (int)t != 255) //if on the ice
                            {
                                bool onRightTeam = (((t == HQMTeam.Blue && BlueTeam.Contains(name)) || (t == HQMTeam.Red && RedTeam.Contains(name))) && LoginManager.IsLoggedIn(name, i));
                                if (!onRightTeam)
                                {
                                    int team = (int)t;
                                    while (team != 255)
                                    {
                                        MemoryEditor.WriteInt(32, Util.PLAYER_LIST_ADDRESS + i * Util.PLAYER_STRUCT_SIZE + Util.LEG_STATE_OFFSET);
                                        team = MemoryEditor.ReadBytes(Util.PLAYER_LIST_ADDRESS + i * Util.PLAYER_STRUCT_SIZE + Util.TEAM_OFFSET, 1)[0];
                                    }
                                        
                                }                                    
                            }
                        }
                    }
                }                
                Thread.Sleep(Util.TRESSPASS_REMOVER_SLEEP);
            }           
        }





        public void CreateTeams()
        {
            //give prio to people who didn't play last game
            List<RankedPlayer> players = new List<RankedPlayer>();
            List<RankedPlayer> others = new List<RankedPlayer>();
            foreach(RankedPlayer p in LoginManager.LoggedInPlayers)
            {
                if (p.PlayedLastGame)
                    others.Add(p);
                else if(players.Count < 10)
                    players.Add(p);
            }          

            Random r = new Random();
            while(players.Count < Math.Min(10, LoginManager.LoggedInPlayers.Count))
            {
                RankedPlayer newPlayer = others[r.Next(others.Count)];
                others.Remove(newPlayer);
                players.Add(newPlayer);                
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
