namespace bot
{
    internal class HelpCommand : ICommand
    {
        public HelpCommand(Bot bot)
        {
            _bot = bot;
        }

        public string Keyword
        { 
            get => "help"; 
        }

        public string HelpText
        {
            get => 
                "Usage: <botname> help [cmdname]\n" +
                "\n" +
                "This command provides information on all the commands that are included " +
                "in this bot (all of them are written down below).\n" +
                "Note #1: cmdname needs to include the (space separated) module " +
                "qualifier.\n" +
                "Note #2: These help notes use angle brackets `<>` for required parameters " +
                "and square brackets `[]` for optional parameters\n" +
                "Note #3: Command names are sequences of keywords separated *strictly* by " +
                "a single space.\n" +
                "Note #4: Usage hints may seem weird in the way they do not specify the exact " +
                "bot name or module name, using <botname> or some canonical name. That is because " +
                "of the possibility of renaming the modules.\n" +
                "Note #5: All dialogues can be immediately ended by the `terminate` keyword.";
        }

        public bool Execute(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                msg.RawMsg.Channel.SendMessageAsync(HelpText + "\n\n" + _bot.GetHelpString(""));
            }
            else if (msg.Content == "help")
            {
                msg.RawMsg.Channel.SendMessageAsync("Ayy, self reference.\n\n" + HelpText);
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync(_bot.GetHelpString(msg.Content));
            }
            return true;
        }

        Bot _bot;
    }
}
