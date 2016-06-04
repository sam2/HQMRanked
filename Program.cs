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
            
            while(true)
            {
                if(game.InProgress || game.StartingGame)
                    game.RemoveTrespassers();

                if (game.InProgress)
                {                    
                    if(GameInfo.IsGameOver)
                    {
                        game.EndGame();
                    }
                }
                else
                {
                    LoginManager.RemoveLoggedOutPlayers();
                    if (LoginManager.LoggedInPlayers.Count >= RankedGame.MIN_PLAYER_COUNT && !game.StartingGame && GameInfo.Period == 0)
                    {
                        game.StartGameTimer();
                        Chat.SendMessage("---Required player count reached. Game will begin shortly.---");                        
                    }
                }                   

                Command cmd = cmdListener.NewCommand();
                if (cmd != null)
                {
                    LoginManager.HandleNewLogins(cmd);
                    if(cmd.Cmd == "start" && cmd.Sender.IsAdmin)
                    {
                        game.StartGame();
                    }
                    if(cmd.Cmd == "end" && cmd.Sender.IsAdmin)
                    {
                        game.EndGame();
                    }
                    Chat.FlushLastCommand();
                }                
            }
        }

        static void WelcomeMessage()
        {
            Chat.SendMessage("----------------------------------------------------------------------------------");
            Chat.SendMessage("          Waiting for players... "+LoginManager.LoggedInPlayers.Count + " / "+RankedGame.MIN_PLAYER_COUNT);
            Chat.SendMessage("        Type /join <password> to play");
            Chat.SendMessage("   New? check reddit.com/r/hqmgames for details");
            Chat.SendMessage("----------------------------------------------------------------------------------");
        }

        
    }
}
