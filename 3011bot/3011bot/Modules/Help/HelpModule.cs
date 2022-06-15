using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace bot
{
    internal class HelpModule : IModule
    {
        public HelpModule()
        {
            Name = "help";
        }

        public string Name { get; init; }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }

        public bool ProcessCommands(MessageWrapper msg)
        {
            return false;
        }

        public bool ProcessDialogues(MessageWrapper msg)
        {
            return false;
        }

        public bool ProcessTriggers(MessageWrapper msg)
        {
            return false;
        }
    }
}
