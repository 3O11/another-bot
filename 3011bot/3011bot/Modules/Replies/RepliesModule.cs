using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace bot
{
    internal class RepliesModule : IModule
    {
        public RepliesModule()
        {
            Name = "replies";
        }

        public string Name { get; init; }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }

        public void Process(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This will print out all the currently valid replies in this server.");
            }

            if (msg.Content.StartsWith("add"))
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This will start the dialogue that will guide you through adding your own reply.");
            }
        }
    }
}
