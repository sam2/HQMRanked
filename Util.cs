using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HQMRanked
{
    public static class Util
    {
        public const int PLAYER_LIST_ADDRESS = 0x00530A60;
        public const int PLAYER_STRUCT_SIZE = 0x98;
        public const int TEAM_OFFSET = 0x8;
        public const int LEG_STATE_OFFSET = 0x74;
        public const int IP_ADDRESS_LIST = 0x004138C0;
        public const int IP_STRUCT_SIZE = 0x4C;
        public const int IP_PLAYER_SLOT_OFFSET = 0x08;
        public const int PLAYER_NAME_OFFSET = 0x14;

        public static int MAX_PLAYERS = 20;
        public static int LEADERBOARD_MIN_GAMES = 10;
        public static int MAINTHREAD_SLEEP = 100;
        public static int GAME_START_TIMER = 10;
        public static int TRESSPASS_REMOVER_SLEEP = 100;

        public static int MIN_PLAYER_COUNT = 10;
        public static int MERCY_RULE_DIFF = 6;

        public static bool NEW_ACCOUNTS_DISABLED = true;
    }
}
