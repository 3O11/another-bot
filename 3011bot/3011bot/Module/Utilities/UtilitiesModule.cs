using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class UtilitiesModule : ModuleBase
    {
        UtilitiesModule(string keyword)
        {
            Keyword = keyword;
        }

        public static UtilitiesModule MakeModule(string keyword)
        {
            var module = new UtilitiesModule(keyword);

            module._moduleDescription =
                "This module contains various assorted commands " +
                "that don't really have anything in common.";

            module.AddCommand(new PingCommand());

            return module;
        }

        // This is still here temporarily, I'll move/remove it later
        void addTestDialogue(MessageWrapper msg)
        {
            DialogueBase dialogue = new();

            dialogue.AddTransition(
                "start",
                (string state, SocketMessage msg) =>
                {
                    msg.Channel.SendMessageAsync("Starting the dialogue");
                    return "test1";
                }
            );

            dialogue.AddTransition(
                "test1",
                (string state, SocketMessage msg) =>
                {
                    msg.Channel.SendMessageAsync("State test1");
                    return "test2";
                }
            );

            dialogue.AddTransition(
                "test2",
                (string state, SocketMessage msg) =>
                {
                    if (msg.Content == "repeat")
                    {
                        msg.Channel.SendMessageAsync("State test2, Returning to state test1");
                        return "test1";
                    }

                    msg.Channel.SendMessageAsync("State test2, Continuing to state test3");
                    return "test3";
                }
            );

            dialogue.AddTransition(
                "test3",
                (string state, SocketMessage msg) =>
                {
                    msg.Channel.SendMessageAsync("State test3, terminating Dialogue");
                    return "final";
                }
            );

            dialogue.Update(msg.RawMsg);
            AddDialogue(msg.RawMsg.Channel.Id, dialogue);
        }
    }
}
