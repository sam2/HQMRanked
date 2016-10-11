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
        string subreddit = "";
        Uri ratingsPost;
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
                    string[] creds = System.IO.File.ReadAllLines("reddit.txt");
                    
                    _Instance = new RedditReporter(creds[0], creds[1]);
                    _Instance.subreddit = creds[2];
                    _Instance.ratingsPost = new Uri(creds[3]);
                }
                return _Instance;
            }
        }

        public void UpdateRatings()
        {
            Post post = Reddit.GetPost(ratingsPost);
            string text = "\\# | NAME | RATING | GP | W | L | PTS | G | A \n ---------|----------|----------|----------|----------|----------|----------|----------|----------\n";                      

            List<UserData> data = UserSaveData.AllUserData.Select(kvp => kvp.Value).Where(x=>x.GamesPlayed >= Util.LEADERBOARD_MIN_GAMES).ToList().OrderByDescending(x => x.Rating.Mean).ToList();
            int i = 1;
            foreach(UserData d in data)
            {              
                text += i + " | " + d.Name + " | " + Math.Round(d.Rating.Mean, 2)*100 + " | " + d.GamesPlayed + " | " + d.Wins + " | " + (d.GamesPlayed - d.Wins) + " | " + (d.Goals + d.Assists) + " | " + d.Goals + " | " + d.Assists + '\n';
                i++;                    
            }
            data = UserSaveData.AllUserData.Select(kvp => kvp.Value).Where(x => x.GamesPlayed < Util.LEADERBOARD_MIN_GAMES && x.GamesPlayed > 1).ToList().OrderByDescending(x => x.GamesPlayed).ToList();

            foreach (UserData d in data)
            {
                text += "-" + " | " + d.Name + " | " + "UNRANKED" + " | " + d.GamesPlayed + " | " + d.Wins + " | " + (d.GamesPlayed - d.Wins) + " | " + (d.Goals + d.Assists) + " | " + d.Goals + " | " + d.Assists + '\n';
                i++;
            }
            post.EditText(text);            
        }

        public void PostGameResult(RankedGameReport report)
        {
            string redtext =  "| RED  | G | A | CHANGE | NEW RATING  \n ----------|----------|----------|----------|----------\n";
            string bluetext = "| BLUE | G | A | CHANGE | NEW RATING  \n ----------|----------|----------|----------|----------\n";
          
            foreach(RankedGameReport.PlayerStatLine p in report.PlayerStats)
            {
                RankedPlayer rp = LoginManager.LoggedInPlayers.FirstOrDefault(x => x.Name == p.Name);
                string statLine = "|" + p.Name + "|" + p.Goals + "|" + p.Assists + "|" + (rp.UserData.GamesPlayed < Util.LEADERBOARD_MIN_GAMES ? "PLACEMENT \n" : GetRatingString(report.OldRatings[p.Name], report.NewRatings[p.Name])) ;
                if(p.Team == HQMEditorDedicated.HQMTeam.Red)
                {
                    redtext += statLine;
                }
                else if(p.Team == HQMEditorDedicated.HQMTeam.Blue)
                {
                    bluetext += statLine;
                }
            }

            string post = "";
            if (report.Winner == HQMEditorDedicated.HQMTeam.Red)
            {
                post = "Red team wins! ";
                post += report.RedScore + " - " + report.BlueScore;
            }
            else
            {
                post = "Blue team wins! ";
                post += report.BlueScore + " - " + report.RedScore;
            }
            

            post += "\n\n";
            post += "MVP: " + report.MVP.Name + "\n\n";
            post += "Match Quality: " + Math.Round(report.MatchQuality,3) + "\n\n";
            post += redtext + "\n\n";
            post += bluetext + "\n\n";

            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            Reddit.GetSubreddit(subreddit).SubmitTextPost(HQMEditorDedicated.ServerInfo.Name + " - "+easternTime.ToString() + " | " + report.RedScore +" - "+report.BlueScore, post);
        }

        public string GetRatingString(Rating oldRating, Rating newRating)
        {
            double change = (Math.Round(newRating.Mean, 2) - Math.Round(oldRating.Mean, 2)) * 100;
            double result = Math.Round(newRating.Mean, 2) * 100;
            string sign = change >= 0 ? "+" : "";
            return sign + (int)change + " | " + result + '\n';
        }
    }
}
