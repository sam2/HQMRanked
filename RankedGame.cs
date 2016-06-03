using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQMEditorDedicated;

namespace HQMRanked
{
    public class RankedGame
    {
        int MIN_PLAYER_COUNT = 10;

        public bool InProgress = false;

        List<RankedPlayer> RedTeam;
        List<RankedPlayer> BlueTeam;

        IEnumerable<IDictionary<Moserware.Skills.Player, Moserware.Skills.Rating>> TrueSkillTeamModel;

        public void StartGame()
        {           
            ResetGame();            
            RemoveTrespassers();
            CreateTeams();
            TrueSkillTeamModel = RatingCalculator.BuildTeamModel(RedTeam, BlueTeam);
            Chat.SendMessage("Game Starting with match quality " + RatingCalculator.CalculateMatchQuality(TrueSkillTeamModel));
            InProgress = true;
            Tools.ResumeGame();
        }

        public void EndGame()
        {
            Chat.SendMessage("Game over. Check reddit.com/r/hqmgames for results");
            var allPlayers = RedTeam.Concat(BlueTeam);
            foreach(RankedPlayer p in allPlayers)
            {
                UserData u;
                if(UserSaveData.AllUserData.TryGetValue(p.HQMPlayer.Name, out u))
                {
                    u.Goals += p.HQMPlayer.Goals;
                    u.Assists += p.HQMPlayer.Assists;
                    u.GamesPlayed++;
                }
            }
            
            if(GameInfo.RedScore > GameInfo.BlueScore)
            {                
                foreach(RankedPlayer p in RedTeam)
                {
                    UserData u;
                    if (UserSaveData.AllUserData.TryGetValue(p.HQMPlayer.Name, out u))
                    {
                        u.Wins++;
                    }
                }
                RatingCalculator.ApplyNewRatings(TrueSkillTeamModel, 1, 2);
            }                
            else if(GameInfo.BlueScore > GameInfo.RedScore)
            {                
                foreach (RankedPlayer p in BlueTeam)
                {
                    UserData u;
                    if (UserSaveData.AllUserData.TryGetValue(p.HQMPlayer.Name, out u))
                    {
                        u.Wins++;
                    }
                }
                RatingCalculator.ApplyNewRatings(TrueSkillTeamModel, 2, 1);
            }                
            else
                RatingCalculator.ApplyNewRatings(TrueSkillTeamModel, 1, 1);
            
            ResetGame();
            RedditReporter.Instance.UpdateRatings();
        }

        public void ResetGame()
        {
            ClearTeams();
            InProgress = false;
        }

        public void RemoveTrespassers()
        {
            foreach(Player p in PlayerManager.PlayersInServer)
            {
                RankedPlayer rp = LoginManager.IsLoggedIn(p);               
                if( (rp == null || p.Team != rp.AssignedTeam) && p.Team != HQMTeam.NoTeam)
                {
                    p.LegState = 32;
                }
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
                    RedTeam.Add(p);
                    p.AssignedTeam = HQMTeam.Red;                    
                }
                else if (BlueTeam.Count < half_max)
                {
                    BlueTeam.Add(p);
                    p.AssignedTeam = HQMTeam.Blue;                
                }                    
                else
                {
                    RedTeam.Add(p);
                    p.AssignedTeam = HQMTeam.Red;                 
                }            
            }

            PrintTeams();

            //auto join
            while (SortedRankedPlayers.Where(p => p.HQMPlayer.Team == HQMTeam.NoTeam).Count() > 0)
            {
                RemoveTrespassers();
                foreach (RankedPlayer p in SortedRankedPlayers.Where(p => p.HQMPlayer.Team == HQMTeam.NoTeam))
                {
                    p.HQMPlayer.LockoutTime = 0;
                    if (p.AssignedTeam == HQMTeam.Red)
                    {
                        p.HQMPlayer.LegState = 4;
                    }
                    else if (p.AssignedTeam == HQMTeam.Blue)
                        p.HQMPlayer.LegState = 8;
                }
            }
        }

        void ClearTeams()
        {
            RedTeam = new List<RankedPlayer>();
            BlueTeam = new List<RankedPlayer>();
            foreach(RankedPlayer p in LoginManager.LoggedInPlayers)
            {
                p.AssignedTeam = HQMTeam.NoTeam;
            }
        }

        double TotalRating(List<RankedPlayer> list)
        {
            double result = 0;
            foreach(RankedPlayer p in list)
            {
                result += p.UserData.Rating.ConservativeRating;
            }
            return result;
        }      

        public void PrintTeams()
        {
            string red = "Red: ";
            foreach (RankedPlayer p in RedTeam)
            {
                red += p.HQMPlayer.Name + " ";
            }

            string blue = "Blue: ";
            foreach (RankedPlayer p in BlueTeam)
            {
                blue += p.HQMPlayer.Name + " ";
            }
            Chat.SendMessage(red);
            Chat.SendMessage(blue);
        }
    }
}
