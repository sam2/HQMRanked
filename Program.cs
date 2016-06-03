using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQMEditorDedicated;

namespace HQMRanked
{
    class Program
    {
        static void Main(string[] args)
        {
            

            Console.WriteLine("Looking for server...");
            while (!MemoryEditor.Init()) { }
            Console.WriteLine("Server found.");

            CommandListener cmdListener = new CommandListener(Chat.MessageCount);
            Chat.RecordCommandSource();

            Console.WriteLine("Reading user data...");
            UserSaveData.AllUserData = UserSaveData.ReadUserData();
            Console.WriteLine("done.");

            Tools.PauseGame();
          

            RankedGame game = new RankedGame();
            Chat.FlushLastCommand();         

            WelcomeMessage();
            while(true)
            {
                if(game.InProgress)
                    game.RemoveTrespassers();
                
                LoginManager.UpdateLoggedInPlayers();
                Command cmd = cmdListener.NewCommand();
                if (cmd != null)
                {
                    LoginManager.HandleNewLogins(cmd);
                    if(cmd.Cmd == "start")
                    {
                        game.StartGame();
                    }
                    if(cmd.Cmd == "end")
                    {
                        game.EndGame();
                    }
                    Chat.FlushLastCommand();
                }



                /*
                if (LoginManager.LoggedInPlayers.Count >= MIN_PLAYER_COUNT)
                {
                    Chat.SendMessage("Player count reached. Game will begin shortly.");
                    game.StartGame();
                }                
                 * */
                
            }
        }

        static void WelcomeMessage()
        {
            Chat.SendMessage("-----------------------------------------------------------------------------");
            Chat.SendMessage("Waiting for players... Type /join <password> to play.");
            Chat.SendMessage("   New? check reddit.com/r/hqmgames for details");
            Chat.SendMessage("-----------------------------------------------------------------------------");
        }
    }
}
