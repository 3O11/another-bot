using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class HelpCommand : ICommand
    {
        public HelpCommand(Bot bot)
        {
            _bot = bot;
        }

        public string Keyword { get => "help"; }

        public string HelpText { get => 
                "Usage: <botname> help [cmdname]\n" +
                "\n" +
                "This command provides information on all the commands that are included " +
                "in this bot (all of them are written down below).\n" +
                "Note #1: <cmdname> needs to include the (space separated) module " +
                "qualifier.\n" +
                "Note #2: These help notes use angle brackets `<>` for required parameters " +
                "and square brackets `[]` for optional parameters";
        }

        public bool Execute(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                msg.RawMsg.Channel.SendMessageAsync(HelpText + "\n\n" + _bot.GetHelpMessage(""));
            }
            else if (msg.Content == "help")
            {
                msg.RawMsg.Channel.SendMessageAsync("Ayy, self reference.\n\n" + HelpText);
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync(_bot.GetHelpMessage(msg.Content));
            }
            return true;
        }

        Bot _bot;
    }
}
