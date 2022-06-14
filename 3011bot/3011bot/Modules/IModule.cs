using Discord;
using Discord.WebSocket;

namespace bot
{
    internal interface IModule
    {
        string Name { get; }
        bool ProcessDialogues(MessageWrapper msg);
        bool ProcessCommands(MessageWrapper msg);
        bool ProcessTriggers(MessageWrapper msg);
        string GetHelp();
    }
}
