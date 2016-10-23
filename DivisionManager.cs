using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQMRanked
{
    class DivisionManager
    {       
        enum EDivision
        {
            Pubstar = 1,
            Gold = 2,
            Silver = 3,
            Bronze = 4,
            Copper = 5,
            Wood = 6,
            UNRANKED = 99
        }

        public static void AssignDivisions()
        {
            List<UserData> rankedPlayers = UserSaveData.AllUserData.Values.Where(x => x.GamesPlayed >= Util.LEADERBOARD_MIN_GAMES).OrderByDescending(x => x.Rating.Mean).ToList();

            float[] k_DivisionNumbers = { rankedPlayers.Count*0.04f, rankedPlayers.Count * 0.23f, rankedPlayers.Count * 0.23f, rankedPlayers.Count * 0.23f, rankedPlayers.Count * 0.23f, rankedPlayers.Count };
            int numSkipped = 0;            

            for(int i = 0; i < k_DivisionNumbers.Count(); i++)
            {
                rankedPlayers.Skip(numSkipped).Take((int)Math.Round(k_DivisionNumbers[i])).ToList().ForEach(x => { x.Division = i + 1; });                
                numSkipped += (int)k_DivisionNumbers[i];
            }

            UserSaveData.AllUserData.Values.Where(x => x.GamesPlayed < Util.LEADERBOARD_MIN_GAMES).ToList().ForEach(x => { x.Division = (int)EDivision.UNRANKED; });            
        }

        
    }
}
