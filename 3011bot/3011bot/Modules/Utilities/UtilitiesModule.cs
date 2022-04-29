using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class UtilitiesModule : IModule
    {
        public UtilitiesModule()
        {
            Name = "utils";
        }

        public string Name { get; init; }

        public bool CanProcess(SocketMessage msg)
        {
            throw new NotImplementedException();
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }

        public void Process(MessageWrapper msg)
        {
            if (msg.Content.StartsWith("ping"))
            {
                msg.RawMsg.Channel.SendMessageAsync("Pong!");
            }
        }
    }
}
