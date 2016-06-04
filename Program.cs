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
        static RankedGame game = new RankedGame();
        static bool startingGame = false;

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
            
            Chat.FlushLastCommand();
            
            while(true)
            {
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
                    if(LoginManager.LoggedInPlayers.Count >= RankedGame.MIN_PLAYER_COUNT && !startingGame && GameInfo.Period == 0)
                    {
                        startingGame = true;
                        StartGameTimer();
                        Chat.SendMessage("---Required player count reached. Game will begin shortly.---");                        
                    }
                }                    
                
                LoginManager.UpdateLoggedInPlayers();
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

        static void StartGameTimer()
        {
            System.Timers.Timer _timer;
            _timer = new System.Timers.Timer(20000);

            _timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
            _timer.AutoReset = false;
            _timer.Enabled = true;
        }   

        static void TimerElapsed(object sender, EventArgs e)
        {
            
            if (LoginManager.LoggedInPlayers.Count < RankedGame.MIN_PLAYER_COUNT)
            {
                Chat.SendMessage("Not enough players. Aborting game...");
                WelcomeMessage();
                return;
            }
            else
            {
                game.StartGame();
            }            
            startingGame = false;
        }

    }
}
