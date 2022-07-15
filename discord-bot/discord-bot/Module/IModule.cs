namespace bot
{
    internal interface IModule
    {
        string Keyword { get; }
        bool ProcessDialogues(MessageWrapper msg);
        void AddCommand(ICommand command);
        void ProcessCommand(MessageWrapper msg);
        bool ProcessTriggers(MessageWrapper msg);
        string GetHelpString(string commandKeyword = "");
        string GetCommandNames();
    }
}
