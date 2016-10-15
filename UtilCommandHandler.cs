using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQMEditorDedicated;

namespace HQMRanked
{
    class UtilCommandHandler
    {
        public static void HandleUtilCommand(Command cmd)
        {
            if(cmd.Sender.IsAdmin)
            {
                if (cmd.Cmd == "postratings")
                {
                    Chat.SendMessage("Updating rankings...");
                    DivisionManager.AssignDivisions();
                    UserSaveData.SaveUserData();
                    RedditReporter.Instance.PostLeagues();
                    Chat.SendMessage("Rankings updated.");
                }
                if (cmd.Cmd == "newaccounts")
                {
                    Util.NEW_ACCOUNTS_DISABLED = !Util.NEW_ACCOUNTS_DISABLED;
                    Chat.SendMessage("NEW_ACCOUNTS_DISABLED=" + Util.NEW_ACCOUNTS_DISABLED);
                }
                if (cmd.Args.Length > 0)
                {
                    if (cmd.Cmd == "tf")
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.TRESSPASS_REMOVER_SLEEP = num;
                            Chat.SendMessage("TRESSPASS_REMOVER_SLEEP set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "mp")
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.MIN_PLAYER_COUNT = num;
                            Chat.SendMessage("MIN_PLAYER_COUNT set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "ts")
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.MAINTHREAD_SLEEP = num;
                            Chat.SendMessage("MAINTHREAD_SLEEP set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "mg")
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.LEADERBOARD_MIN_GAMES = num;
                            Chat.SendMessage("LEADERBOARD_MIN_GAMES set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "gs")
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.GAME_START_TIMER = num;
                            Chat.SendMessage("GAME_START_TIMER set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "changepw" && cmd.Args.Length > 1)
                    {
                        UserData u;
                        if (UserSaveData.AllUserData.TryGetValue(cmd.Args[0], out u))
                        {
                            u.Password = cmd.Args[1];
                            Chat.SendMessage(cmd.Args[0] + " password changed");
                        }
                    }
                    else if (cmd.Cmd == "resetaccount")
                    {
                        if (UserSaveData.AllUserData.ContainsKey(cmd.Args[0]) && !LoginManager.LoggedInPlayers.Any(x => x.Name == cmd.Args[0]))
                        {
                            UserSaveData.AllUserData.Remove(cmd.Args[0]);
                            Chat.SendMessage(cmd.Args[0] + " userdata deleted.");
                        }
                    }
                    else if (cmd.Cmd == "mr")
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.MERCY_RULE_DIFF = num;
                            Chat.SendMessage("MERCY_RULE_DIFF set to " + num);
                        }
                    }                
                }              
            }
        }
    }
}
