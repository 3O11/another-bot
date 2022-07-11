namespace bot
{
    internal interface ICommand
    {
        public string Keyword { get; }
        public string HelpText { get; }
        public bool Execute(MessageWrapper msg);
    }
}
