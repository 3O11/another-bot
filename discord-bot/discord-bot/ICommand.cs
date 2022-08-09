namespace bot
{
    /// <summary>
    /// Keyword is used by the Module/Bot to pick the command.
    /// HelpText is the text that gets sent when the user asks for help with a particular command.
    /// Execute() is the function called when the corresponding command is called.
    /// </summary>
    internal interface ICommand
    {
        public string Keyword { get; }
        public string HelpText { get; }
        public void Execute(MessageWrapper msg);
    }
}
