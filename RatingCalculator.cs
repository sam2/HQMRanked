using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moserware.Skills;

namespace HQMRanked
{
    class RatingCalculator
    {
        static GameInfo gameInfo = GameInfo.DefaultGameInfo;
        public static IEnumerable<IDictionary<Player, Rating>> BuildTeamModel(List<RankedPlayer> RedTeam, List<RankedPlayer> BlueTeam)
        {         
            var redTeam = new Team();
            foreach(RankedPlayer p in RedTeam)
            {
                redTeam.AddPlayer(new Player(p.HQMPlayer.Name), p.UserData.Rating);
            }

            var blueTeam = new Team();
            foreach(RankedPlayer p in BlueTeam)
            {
                blueTeam.AddPlayer(new Player(p.HQMPlayer.Name), p.UserData.Rating);
            }
            return Teams.Concat(redTeam, blueTeam);            
        }

        public static double CalculateMatchQuality(IEnumerable < IDictionary < Player, Rating >> teamModel)
        {
            return TrueSkillCalculator.CalculateMatchQuality(gameInfo, teamModel);
        }

        public static void ApplyNewRatings(IEnumerable<IDictionary<Player, Rating>> teamModel, int redRank, int blueRank)
        {
            IDictionary<Player, Rating> newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teamModel, redRank, blueRank);
            foreach(KeyValuePair<Player, Rating> kvp in newRatings)
            {
                UserData u;
                if (UserSaveData.AllUserData.TryGetValue((string)kvp.Key.Id, out u))
                {
                    u.Rating = kvp.Value;
                }
                else
                    Console.WriteLine("could not find player: " + (string)kvp.Key.Id);
            }
            UserSaveData.SaveUserData();

           
        }

        


        
    }
}
