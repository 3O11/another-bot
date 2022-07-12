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

        public void Execute(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                msg.RawMsg.Channel.SendMessageAsync("Pong!");
            }
        }
    }
}
