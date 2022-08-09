namespace bot
{
    /// <summary>
    /// Modules can process Dialogues, Commands and Triggers,
    /// they can also have their own custom help message.
    /// </summary>
    internal interface IModule
    {
        string Keyword { get; }
        bool ProcessDialogues(MessageWrapper msg);
        void ProcessCommand(MessageWrapper msg);
        bool ProcessTriggers(MessageWrapper msg);
        string GetHelpString(string commandKeyword = "");
        string GetCommandNames();
    }
}
