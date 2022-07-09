using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class PingCommand : ICommand
    {
        public string Keyword
        {
            get =>
                "ping";
        }

        public string HelpText
        {
            get =>
                "Usage: <botname> ping\n\n" +
                "Replies with Pong!";
        }

        public bool Execute(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                msg.RawMsg.Channel.SendMessageAsync("Pong!");
            }
            return true;
        }
    }
}
