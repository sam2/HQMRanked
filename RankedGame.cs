﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using HQMEditorDedicated;

namespace HQMRanked
{
    public class RankedGame
    {
        
        public static int MIN_PLAYER_COUNT = 2;

        public bool InProgress = false;
        public bool StartingGame
        {
            get;
            private set;
        }

        List<string> RedTeam = new List<string>();
        List<string> BlueTeam = new List<string>();

        IEnumerable<IDictionary<Moserware.Skills.Player, Moserware.Skills.Rating>> TrueSkillTeamModel;
        System.Timers.Timer _timer;

        public void StartGameTimer()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
            _timer.AutoReset = false;
            _timer.Enabled = true;
            StartingGame = true;
        }

        void TimerElapsed(object sender, EventArgs e)
        {
            if (LoginManager.LoggedInPlayers.Count < MIN_PLAYER_COUNT)
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
            InProgress = true;
            ClearTeams();         
            CreateTeams();
            TrueSkillTeamModel = RatingCalculator.BuildTeamModel(RedTeam, BlueTeam);
            Chat.SendMessage("Game Starting with match quality " + Math.Round(RatingCalculator.CalculateMatchQuality(TrueSkillTeamModel), 2));            
            Tools.ResumeGame();
        }

        public void EndGame(bool record)
        {            
            Chat.SendMessage("Game over. Check reddit.com/r/hqmgames for results");
            if(record)
            {
                IDictionary<Moserware.Skills.Player, Moserware.Skills.Rating> newRatings;
                var allPlayers = RedTeam.Concat(BlueTeam);
                foreach (string p in allPlayers)
                {
                    UserData u;
                    if (UserSaveData.AllUserData.TryGetValue(p, out u))
                    {
                        RankedPlayer rp = LoginManager.LoggedInPlayers.FirstOrDefault(x => x.Name == p);
                        if(rp != null)
                        {
                            u.Goals += rp.PlayerStruct.Goals;
                            u.Assists += rp.PlayerStruct.Assists;
                            u.GamesPlayed++;
                        }
                        
                    }
                }

                if (GameInfo.RedScore > GameInfo.BlueScore)
                {
                    foreach (string p in RedTeam)
                    {
                        UserData u;
                        if (UserSaveData.AllUserData.TryGetValue(p, out u))
                        {
                            u.Wins++;
                        }
                    }
                    newRatings = Moserware.Skills.TrueSkillCalculator.CalculateNewRatings(RatingCalculator.GameInfo, TrueSkillTeamModel, 1, 2);
                    
                }
                else
                {
                    foreach (string p in BlueTeam)
                    {
                        UserData u;
                        if (UserSaveData.AllUserData.TryGetValue(p, out u))
                        {
                            u.Wins++;
                        }
                    }
                    newRatings = Moserware.Skills.TrueSkillCalculator.CalculateNewRatings(RatingCalculator.GameInfo, TrueSkillTeamModel, 2, 1);
                }
                
                RedditReporter.Instance.PostGameResult(GameInfo.RedScore, GameInfo.BlueScore, RedTeam, BlueTeam, 0, newRatings);
                RatingCalculator.ApplyNewRatings(newRatings);
                RedditReporter.Instance.UpdateRatings();
            }
            
            ClearTeams();
            GameInfo.IsGameOver = true;
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
            List<RankedPlayer> SortedRankedPlayers = LoginManager.LoggedInPlayers.OrderByDescending(x => x.UserData.Rating.ConservativeRating).ToList();
            while(SortedRankedPlayers.Count > 10)
            {
                SortedRankedPlayers.RemoveAt(new Random().Next(SortedRankedPlayers.Count));
            }

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
            while (SortedRankedPlayers.Where(p => p.PlayerStruct.Team == HQMTeam.NoTeam).Count() > 0)
            {
                foreach (RankedPlayer p in SortedRankedPlayers.Where(p => p.PlayerStruct.Team == HQMTeam.NoTeam))
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
            RedTeam = new List<string>();
            BlueTeam = new List<string>();
            foreach(RankedPlayer p in LoginManager.LoggedInPlayers)
            {
                p.AssignedTeam = HQMTeam.NoTeam;
            }
        }

        double TotalRating(List<string> list)
        {
            double result = 0;
            foreach(string p in list)
            {
                result += UserSaveData.AllUserData[p].Rating.Mean;
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
    }
}
