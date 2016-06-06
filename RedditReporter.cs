using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;

namespace HQMRanked
{
    class RedditReporter
    {
        Reddit Reddit;

        public RedditReporter(string username, string password)
        {
            Reddit = new Reddit(username, password);
        }

        private static RedditReporter _Instance;
        public static RedditReporter Instance
        {
            get
            {
                if(_Instance == null)
                {
                    _Instance = new RedditReporter("", "");
                }
                return _Instance;
            }
        }

        public void UpdateRatings()
        {
            Post post = Reddit.GetPost(new Uri("https://www.reddit.com/r/hqmgames/comments/4mepb1/ratings/"));
            string text = "\\# | NAME | RATING | GP | W | L | PTS | G | A \n ---------|----------|----------|----------|----------|----------|----------|----------|----------\n";                      

            List<UserData> data = UserSaveData.AllUserData.Select(kvp => kvp.Value).ToList().OrderByDescending(x => x.Rating.ConservativeRating).ToList();
            int i = 1;
            foreach(UserData d in data)
            {
                if(d.GamesPlayed > Util.LEADERBOARD_MIN_GAMES)
                {
                    text += i + " | " + d.Name + " | " + Math.Round(d.Rating.ConservativeRating, 2) + " | " + d.GamesPlayed + " | " + d.Wins + " | " + (d.GamesPlayed - d.Wins) + " | " + (d.Goals + d.Assists) + " | " + d.Goals + " | " + d.Assists + '\n';
                    i++;
                }
                    
            }
            post.EditText(text);            
        }

        public void PostGameResult(int redScore, int blueScore, List<string> redTeam, List<string> blueTeam, double matchQuality, IDictionary<Moserware.Skills.Player, Moserware.Skills.Rating> newRatings)
        {
            string redtext = "| RED | G | A | RATING CHANGE \n ----------|----------|----------|----------\n";
            for(int i = 0; i < redTeam.Count; i++)
            {
                HQMEditorDedicated.Player p = LoginManager.LoggedInPlayers.FirstOrDefault(x => x.Name == redTeam[i]).PlayerStruct;
                redtext += "|" + p.Name + "|" + p.Goals + "|" + p.Assists + "|" + UserSaveData.AllUserData[redTeam[i]].Rating.ToString() + "=>" + newRatings.Where(x => x.Key.Id == redTeam[i]).ToString() + '\n';
            }

            string blueText = "| BLUE | G | A | RATING CHANGE \n ----------|----------|----------|----------\n";
            for(int i = 0; i < blueTeam.Count; i++)
            {
                HQMEditorDedicated.Player p = LoginManager.LoggedInPlayers.FirstOrDefault(x => x.Name == redTeam[i]).PlayerStruct;
                blueText += "|" + p.Name + "|" + p.Goals + "|" + p.Assists + "|" + UserSaveData.AllUserData[blueTeam[i]].Rating.ToString() + "=>" + newRatings.Where(x => x.Key.Id == p.Name).ToString() + '\n';
            }

            string post = "";
            if (redScore > blueScore)
            {
                post = "Red team wins! ";
                post += redScore + " - " + blueScore;
            }
            else
            {
                post = "Blue team wins! ";
                post += blueScore + " - " + redScore;
            }
            post += "\n\n";
            post += redtext + "\n\n";
            post += blueText + "\n\n";

            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            Reddit.GetSubreddit("hqmgames").SubmitTextPost(HQMEditorDedicated.ServerInfo.Name + " - "+easternTime.ToString() + " | " + redScore +" - "+blueScore, post);
                
            
        }
    }
}
