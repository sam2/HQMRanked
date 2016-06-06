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
        public static GameInfo GameInfo = GameInfo.DefaultGameInfo;
        public static IEnumerable<IDictionary<Player, Rating>> BuildTeamModel(List<string> RedTeam, List<string> BlueTeam)
        {         
            var redTeam = new Team();
            foreach(string p in RedTeam)
            {
                redTeam.AddPlayer(new Player(p), UserSaveData.AllUserData[p].Rating);
            }

            var blueTeam = new Team();
            foreach(string p in BlueTeam)
            {
                blueTeam.AddPlayer(new Player(p), UserSaveData.AllUserData[p].Rating);
            }
            return Teams.Concat(redTeam, blueTeam);            
        }

        public static double CalculateMatchQuality(IEnumerable < IDictionary < Player, Rating >> teamModel)
        {
            return TrueSkillCalculator.CalculateMatchQuality(GameInfo, teamModel);
        }

        public static void ApplyNewRatings(IDictionary<Player, Rating> newRatings)
        {
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
