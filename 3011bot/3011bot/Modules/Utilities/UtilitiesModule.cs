using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class UtilitiesModule : DialogueModuleBase
    {
        public UtilitiesModule()
        {
            Name = "utils";
        }

        public override string GetHelp()
        {
            throw new NotImplementedException();
        }

        public override bool ProcessCommandsExt(MessageWrapper msg)
        {
            if (msg.Content == "ping")
            {
                msg.RawMsg.Channel.SendMessageAsync("Pong!");
                return true;
            }
            else if (msg.Content == "testDialogue")
            {
                msg.RawMsg.Channel.SendMessageAsync("Starting generic dialogue");
                addDialogue(msg.RawMsg.Channel.Id, new GenericDialogue());
                return true;
            }

            return false;
        }
    }
}
