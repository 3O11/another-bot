using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace bot
{
    internal interface IModule
    {
        string Keyword { get; }
        bool ProcessDialogues(MessageWrapper msg);
        bool ProcessCommands(MessageWrapper msg);
        bool ProcessTriggers(MessageWrapper msg);
        string GetHelpString(string commandKeyword = "");
        string GetCommandNames();
    }
}
