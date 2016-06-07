using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;
using Moserware.Skills;

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

        public void PostGameResult(RankedGameReport report)
        {
            string redtext =  "| RED  | G | A | OLD RATING | NEW RATING \n ----------|----------|----------|----------|----------\n";
            string bluetext = "| BLUE | G | A | OLD RATING | NEW RATING \n ----------|----------|----------|----------|----------\n";
          
            foreach(RankedGameReport.PlayerStatLine p in report.PlayerStats)
            {
                if(p.Team == HQMEditorDedicated.HQMTeam.Red)
                {
                    redtext += "|" + p.Name + "|" + p.Goals + "|" + p.Assists + "|" + GetRatingString(report.OldRatings[p.Name], report.NewRatings[p.Name]);
                }
                else if(p.Team == HQMEditorDedicated.HQMTeam.Blue)
                {
                    bluetext += "|" + p.Name + "|" + p.Goals + "|" + p.Assists + "|" + GetRatingString(report.OldRatings[p.Name], report.NewRatings[p.Name]);
                }
            }

            string post = "";
            if (report.Winner == HQMEditorDedicated.HQMTeam.Red)
            {
                post = "Red team wins! ";
                post += report.RedScore + " - " + report.RedScore;
            }
            else
            {
                post = "Blue team wins! ";
                post += report.BlueScore + " - " + report.RedScore;
            }
            post += "\n\n";
            post += "Match Quality: " + report.MatchQuality;
            post += redtext + "\n\n";
            post += bluetext + "\n\n";

            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            Reddit.GetSubreddit("hqmgames").SubmitTextPost(HQMEditorDedicated.ServerInfo.Name + " - "+easternTime.ToString() + " | " + report.RedScore +" - "+report.BlueScore, post);
        }

        public string GetRatingString(Rating oldRating, Rating newRating)
        {
            return "**"+Math.Round(oldRating.ConservativeRating, 2)+ "**" + " (*" + oldRating.ToString() + "*)" + " | " + "**" + Math.Round(newRating.ConservativeRating, 2)+"**"+" (*" + newRating.ToString() + "*)" + '\n';
        }
    }
}
