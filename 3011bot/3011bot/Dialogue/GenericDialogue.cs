using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class GenericDialogue : IDialogue
    {
        public bool Update(SocketMessage msg)
        {
            if (msg.Content == "Exit")
            {
                msg.Channel.SendMessageAsync("Exiting the dialogue.");
                return true;
            }

            msg.Channel.SendMessageAsync("The dialogue is still going.");

            return false;
        }
    }
}
