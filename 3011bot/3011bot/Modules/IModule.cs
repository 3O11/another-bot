using Discord;
using Discord.WebSocket;

namespace bot
{
    internal interface IModule
    {
        string Name { get; init; }
        void Process(MessageWrapper msg);
        string GetHelp();
    }
}
