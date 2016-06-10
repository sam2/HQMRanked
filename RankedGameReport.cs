using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQMEditorDedicated;

namespace HQMRanked
{
    public class RankedGameReport
    {
        public class PlayerStatLine
        {
            public string Name;
            public HQMTeam Team;
            public int Goals;
            public int Assists;
            public bool Leaver = false;
        }

        public List<PlayerStatLine> PlayerStats;
        public int RedScore;
        public int BlueScore;
        public HQMTeam Winner;
        public PlayerStatLine MVP;
        public double MatchQuality;
        public IDictionary<string, Moserware.Skills.Rating> OldRatings;
        public IDictionary<string, Moserware.Skills.Rating> NewRatings;

        //To be created when game ends. Assumes before Reset.
        public RankedGameReport(List<string> RedTeam, List<String> BlueTeam, IEnumerable<IDictionary<string, Moserware.Skills.Rating>> teamModel)
        {
            RedScore = GameInfo.RedScore;
            BlueScore = GameInfo.BlueScore;
            Winner = RedScore > BlueScore ? HQMTeam.Red : HQMTeam.Blue;

            MatchQuality = Moserware.Skills.TrueSkillCalculator.CalculateMatchQuality(RatingCalculator.GameInfo, teamModel);
            PlayerStats = CreateStatLines(RedTeam, BlueTeam);
            OldRatings = GetOldRatings(RedTeam.Concat(BlueTeam));
            NewRatings = GetNewRatings(teamModel);

            MVP = PlayerStats[0];
            foreach(PlayerStatLine p in PlayerStats)
            {
                if(p.Goals + p.Assists > MVP.Goals + MVP.Assists)
                {
                    MVP = p;
                }
            }
        }

        private List<PlayerStatLine> CreateStatLines(List<string> RedTeam, List<String> BlueTeam)
        {
            List<PlayerStatLine> stats = new List<PlayerStatLine>();
            foreach (string s in RedTeam.Concat(BlueTeam))
            {
                PlayerStatLine player = new PlayerStatLine();
                player.Name = s;
                player.Team = RedTeam.Contains(s) ? HQMTeam.Red : HQMTeam.Blue;

                RankedPlayer rp = LoginManager.LoggedInPlayers.FirstOrDefault(x => x.Name == s);
                if (rp != null && rp.Name == rp.PlayerStruct.Name && rp.PlayerStruct.InServer)
                {
                    player.Goals = rp.PlayerStruct.Goals;
                    player.Assists = rp.PlayerStruct.Assists;
                }
                else
                {
                    player.Leaver = true;
                }

                stats.Add(player);
            }
            return stats;
        }

        private IDictionary<string, Moserware.Skills.Rating> GetOldRatings(IEnumerable<string> players)
        {
            IDictionary<string, Moserware.Skills.Rating> oldRatings = new Dictionary<string, Moserware.Skills.Rating>();
            foreach(string p in players)
            {
                Moserware.Skills.Rating rating = UserSaveData.AllUserData[p].Rating;
                oldRatings[p] = rating;
            }
            return oldRatings;
        }

        private IDictionary<string, Moserware.Skills.Rating> GetNewRatings(IEnumerable<IDictionary<string, Moserware.Skills.Rating>> teamModel)
        {
            if(Winner == HQMTeam.Red)
                return Moserware.Skills.TrueSkillCalculator.CalculateNewRatings(Moserware.Skills.GameInfo.DefaultGameInfo, teamModel, 1, 2);
            else
                return Moserware.Skills.TrueSkillCalculator.CalculateNewRatings(Moserware.Skills.GameInfo.DefaultGameInfo, teamModel, 2, 1);
        }
    }

    
}
