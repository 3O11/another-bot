using System.Collections.Generic;
using Discord.WebSocket;

namespace bot
{
    internal static class Utils
    {
        public static SocketGuild GetGuild(SocketMessage msg)
        {
            // https://github.com/discord-net/Discord.Net/issues/621
            // I am aware that there is a possibility that the conversion
            // might return null, but as far as I was able to check, this
            // is currently the only implementation of the interface that
            // can occur in a SocketMessage.
            var channel = msg.Channel as SocketGuildChannel;
            return channel.Guild;
        }

        public static List<ulong>? ExtractIds(string str)
        {
            List<ulong>? ids = new();
            ulong buffer = 0;
            foreach (char ch in str)
            {
                if (ch >= '0' && ch <= '9')
                {
                    buffer = (buffer * 10) + (ulong)(ch - '0');
                }
                else if (buffer != 0)
                {
                    ids.Add(buffer);
                    buffer = 0;
                }
            }

            if (buffer != 0)
            {
                ids.Add(buffer);
            }

            return ids.Count > 0 ? ids : null;
        }

        public static string ExtractFirstKeyword(string str, int offset = 0)
        {
            int spacePos = str.IndexOf(' ', offset);
            return str.Substring(offset, (spacePos < 0 ? str.Length : spacePos) - offset);
        }
    }
}
