using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQMEditorDedicated;

namespace HQMRanked
{
    public class CommandListener
    {
        int messageCount;

        public CommandListener(int msgCount)
        {
            messageCount = msgCount;
        }

        public Command NewCommand()
        {     
            Chat.ChatMessage lastCommand = Chat.LastCommand;
            if (lastCommand != null && lastCommand.Message.Length > 0 && lastCommand.Message[0] == '/')
            {                
                string[] cmdstring = lastCommand.Message.Substring(1).Split(' ');
                string cmd = cmdstring[0];
                string[] args = cmdstring.Skip(1).ToArray();                
                return new Command(lastCommand.Sender, cmd, args);
            }
            return null;
        }
    }

    public class Command
    {
        public Player Sender;
        public string Cmd;
        public string[] Args;

        public Command(Player p, string cmd, string[] args)
        {
            Sender = p;
            Cmd = cmd;
            Args = args;
        }
    }
}
