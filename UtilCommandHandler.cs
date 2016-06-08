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
            if(cmd.Sender.IsAdmin && cmd.Args.Length > 0)
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
            }
            
        }
    }
}
