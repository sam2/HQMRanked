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
        public static IEnumerable<IDictionary<string, Rating>> BuildTeamModel(List<string> RedTeam, List<string> BlueTeam)
        {         
            var redTeam = new Team<string>();
            foreach(string p in RedTeam)
            {
                redTeam.AddPlayer(p, UserSaveData.AllUserData[p].Rating);
            }

            var blueTeam = new Team<string>();
            foreach(string p in BlueTeam)
            {
                blueTeam.AddPlayer(p, UserSaveData.AllUserData[p].Rating);
            }
            return Teams.Concat(redTeam, blueTeam);            
        }

        public static double CalculateMatchQuality(IEnumerable<IDictionary<string, Rating>> teamModel)
        {
            return TrueSkillCalculator.CalculateMatchQuality(GameInfo, teamModel);
        }   
    }
}
