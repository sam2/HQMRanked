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

            Util.MAX_PLAYERS = ServerInfo.MaxPlayerCount;

            CommandListener cmdListener = new CommandListener(Chat.MessageCount);
            Chat.RecordCommandSource();

            Console.WriteLine("Reading user data...");
            UserSaveData.AllUserData = UserSaveData.ReadUserData();
            Console.WriteLine("done.");

            Tools.PauseGame();
            RankedGame game = new RankedGame();
            Chat.FlushLastCommand();

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
                    if (LoginManager.LoggedInPlayers.Count >= RankedGame.MIN_PLAYER_COUNT && !game.StartingGame && GameInfo.Period == 0)
                    {
                        game.StartGameTimer();
                        Chat.SendMessage("---Required player count reached. Game will begin shortly.---");                        
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
                            Chat.SendMessage("trespasser remove freq set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "mp" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Chat.SendMessage("min player set");
                            RankedGame.MIN_PLAYER_COUNT = num;
                        }
                        
                    }
                    else if (cmd.Cmd == "ts" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if (int.TryParse(cmd.Args[0], out num))
                        {
                            Util.MAINTHREAD_SLEEP = num;
                            Chat.SendMessage("thread sleep set to " + num);
                        }
                    }
                    else if (cmd.Cmd == "mg" && cmd.Sender.IsAdmin && cmd.Args.Length > 0)
                    {
                        int num = 0;
                        if(int.TryParse(cmd.Args[0], out num))
                        {
                            Util.LEADERBOARD_MIN_GAMES = num;
                            Chat.SendMessage("min games set to " + num);
                        }                        
                    }
                    else if(cmd.Cmd == "info" && (!game.InProgress || game.StartingGame))
                    {
                        WelcomeMessage();
                    }
                    else if(cmd.Cmd == "logout")
                    {
                        RankedPlayer p = LoginManager.LoggedInPlayers.FirstOrDefault(x => x.Name == cmd.Sender.Name));
                        if(p!=null)
                        {
                            LoginManager.LoggedInPlayers.Remove(p);
                            Chat.SendMessage(p.Name + " logged out.");
                        }
                            
                    }
                    Chat.FlushLastCommand();
                }
                Thread.Sleep(Util.MAINTHREAD_SLEEP);
            }
            
        }

        static void WelcomeMessage()
        {
            Chat.SendMessage("          Waiting for players... "+LoginManager.LoggedInPlayers.Count + " / "+RankedGame.MIN_PLAYER_COUNT);
            Chat.SendMessage("        Type /join <password> to play");
        }

        
    }
}
