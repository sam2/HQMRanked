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
            int MIN_PLAYER_COUNT = 10;

            Console.WriteLine("Looking for server...");
            while (!MemoryEditor.Init()) { }
            Console.WriteLine("Server found.");

            CommandListener cmdListener = new CommandListener(Chat.MessageCount);
            Chat.RecordCommandSource();

            LoginManager.AllUsers = UserData.ReadUserData();

            Tools.PauseGame();
            Chat.SendMessage("Waiting for players... Type /join <password> to play.");
            Chat.SendMessage("New? check reddit.com/r/hqmgames for details");

            RankedGame game = new RankedGame();
            //Chat.FlushLastCommand();

            while(true)
            {
                if(!game.InProgress)
                {
                    Command cmd = cmdListener.NewCommand();
                    if (cmd != null)
                    {
                        if (cmd.Cmd == "join" && cmd.Args.Length > 0)
                        {
                            if (LoginManager.Login(cmd.Sender.Name, cmd.Args[0]))
                            {
                                Chat.SendMessage("Waiting for players... - " + LoginManager.LoggedInUsers.Count + "/" + MIN_PLAYER_COUNT);
                            }
                            else
                            {
                                LoginManager.CreateNewUser(cmd.Sender.Name, cmd.Args[0]);                                
                                LoginManager.Login(cmd.Sender.Name, cmd.Args[0]);
                                UserData.SaveUserData(LoginManager.AllUsers);
                            }
                        }                        
                        Chat.FlushLastCommand();
                    }

                    

                    if(LoginManager.LoggedInUsers.Count >= MIN_PLAYER_COUNT)
                    {
                        Chat.SendMessage("Player count reached. Game will begin shortly.");
                        game.StartGame();
                    }
                }
                
            }
        }
    }
}
