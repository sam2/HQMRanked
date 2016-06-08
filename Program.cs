using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            Util.MAX_PLAYERS = ServerInfo.MaxPlayerCount;

            Thread removeTresspassers = new Thread(game.RemoveTrespassers);
            removeTresspassers.Start();            
            
            while(true)
            {              
                if (game.InProgress)
                {                    
                    if(GameInfo.IsGameOver)
                    {
                        game.EndGame(true);                        
                    }                    
                }
                else
                {                    
                    if (LoginManager.LoggedInPlayers.Count >= Util.MIN_PLAYER_COUNT && !game.StartingGame && GameInfo.Period == 0)
                    {
                        game.StartGameTimer();
                        Chat.SendMessage("---------------------------------------------------");
                        Chat.SendMessage("     Required player count reached.");
                        Chat.SendMessage("     Game will start in " + Util.GAME_START_TIMER + " seconds.");
                        Chat.SendMessage("---------------------------------------------------");
                    }
                }
                if (LoginManager.LoggedInPlayers.Count > 0)
                    LoginManager.RemoveLoggedOutPlayers();

                Command cmd = cmdListener.NewCommand();
                if (cmd != null)
                {
                    LoginManager.HandleNewLogins(cmd);
                    if(cmd.Cmd == "start" && cmd.Sender.IsAdmin)
                    {
                        game.StartGame();
                    }
                    else if(cmd.Cmd == "end" && cmd.Sender.IsAdmin)
                    {
                        game.EndGame(false);
                    }
                    else if (cmd.Cmd == "tf" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.TRESSPASS_REMOVER_SLEEP = num;
                            Chat.SendMessage("TRESSPASS_REMOVER_SLEEP set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "mp" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {                            
                            Util.MIN_PLAYER_COUNT = num;
                            Chat.SendMessage("MIN_PLAYER_COUNT set to "+num);
                        }
                        
                    }
                    else if (cmd.Cmd == "ts" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.MAINTHREAD_SLEEP = num;
                            Chat.SendMessage("MAINTHREAD_SLEEP set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "mg" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if(int.TryParse(cmd.Args[0], out num))
                        {
                            Util.LEADERBOARD_MIN_GAMES = num;
                            Chat.SendMessage("LEADERBOARD_MIN_GAMES set to " + num);
                        }                        
                    }
                    else if (cmd.Cmd == "gs" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.GAME_START_TIMER = num;
                            Chat.SendMessage("GAME_START_TIMER set to " + num);
                        }
                    }
                    else if(cmd.Cmd == "info" && (!game.InProgress || game.StartingGame))
                    {
                        WelcomeMessage();
                    }
                    
                    Chat.FlushLastCommand();
                }
                Thread.Sleep(Util.MAINTHREAD_SLEEP);
            }
            
        }

        static void WelcomeMessage()
        {
            
            Chat.SendMessage("             Logged in players: "+LoginManager.LoggedInPlayers.Count + " / "+Util.MIN_PLAYER_COUNT);
            Chat.SendMessage("        Type /join <yourpassword> to play");
          
        }

        
    }
}
