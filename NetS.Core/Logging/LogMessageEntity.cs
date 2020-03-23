using System;

namespace NetS.Core.Logging
{
    public struct LogMessageEntry
    {
        public string LevelString;
        public ConsoleColor? LevelBackground;
        public ConsoleColor? LevelForeground;
        public ConsoleColor? MessageColor;
        public string Message;
    }
}
