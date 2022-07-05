using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
