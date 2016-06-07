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
                    string[] raw = System.IO.File.ReadAllLines("reddit.txt");
                    _Instance = new RedditReporter(raw[0], raw[1]);
                }
                return _Instance;
            }
        }

        public void UpdateRatings()
        {
            Post post = Reddit.GetPost(new Uri("https://www.reddit.com/r/hqmgames/comments/4mepb1/ratings/"));
            string text = "\\# | NAME | RATING | GP | W | L | PTS | G | A \n ---------|----------|----------|----------|----------|----------|----------|----------|----------\n";                      

            List<UserData> data = UserSaveData.AllUserData.Select(kvp => kvp.Value).Where(x=>x.GamesPlayed >= Util.LEADERBOARD_MIN_GAMES).ToList().OrderByDescending(x => x.Rating.ConservativeRating).ToList();
            int i = 1;
            foreach(UserData d in data)
            {              
                text += i + " | " + d.Name + " | " + Math.Round(d.Rating.ConservativeRating, 2) + " | " + d.GamesPlayed + " | " + d.Wins + " | " + (d.GamesPlayed - d.Wins) + " | " + (d.Goals + d.Assists) + " | " + d.Goals + " | " + d.Assists + '\n';
                i++;                    
            }
            data = UserSaveData.AllUserData.Select(kvp => kvp.Value).Where(x => x.GamesPlayed < Util.LEADERBOARD_MIN_GAMES && x.GamesPlayed > 0).ToList().OrderByDescending(x => x.GamesPlayed).ToList();
            i = 0;
            foreach (UserData d in data)
            {
                text += i + " | " + d.Name + " | " + "UNRANKED" + " | " + d.GamesPlayed + " | " + d.Wins + " | " + (d.GamesPlayed - d.Wins) + " | " + (d.Goals + d.Assists) + " | " + d.Goals + " | " + d.Assists + '\n';
                i++;
            }
            post.EditText(text);            
        }

        public void PostGameResult(RankedGameReport report)
        {
            string redtext =  "| RED  | G | A | OLD RATING | NEW RATING \n ----------|----------|----------|----------|----------\n";
            string bluetext = "| BLUE | G | A | OLD RATING | NEW RATING \n ----------|----------|----------|----------|----------\n";
          
            foreach(RankedGameReport.PlayerStatLine p in report.PlayerStats)
            {
                string statLine = "|" + p.Name + "|" + p.Goals + "|" + p.Assists + "|" + (p.Leaver? "LEFT GAME" : GetRatingString(report.OldRatings[p.Name], report.NewRatings[p.Name]));
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
                post += report.RedScore + " - " + report.BlueScore + (report.OT? "OT" :"");
            }
            else
            {
                post = "Blue team wins! ";
                post += report.BlueScore + " - " + report.RedScore + (report.OT ? "OT" : "");
            }
            

            post += "\n\n";
            post += "Match Quality: " + report.MatchQuality + "\n\n";
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
