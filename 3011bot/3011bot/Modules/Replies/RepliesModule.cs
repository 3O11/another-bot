using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace bot
{
    internal class RepliesModule : DialogueModuleBase
    {
        public RepliesModule()
        {
            Name = "replies";
            _replyStorage.TryAdd(426274169152471041, new List<Reply> { new Reply() });
        }

        public override string GetHelp()
        {
            throw new NotImplementedException();
        }

        public override bool ProcessCommandsExt(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This will print out all the currently valid replies in this server.");
                return true;
            }

            if (msg.Content.StartsWith("add"))
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This will start the dialogue that will guide you through adding your own reply.");
                return true;
            }

            if (_replyStorage.TryGetValue(Utils.GetGuild(msg.RawMsg).Id, out var replies))
            {
                foreach (var reply in replies)
                {
                    if (reply.Process(msg.RawMsg))
                        return true;
                }
            }

            return false;
        }

        public override bool ProcessTriggers(MessageWrapper msg)
        {
            return base.ProcessTriggers(msg);
        }

        ConcurrentDictionary<ulong, List<Reply>> _replyStorage = new();
    }
}
