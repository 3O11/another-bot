using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class UtilitiesModule : DialogueModuleBase
    {
        public UtilitiesModule()
        {
            Name = "utils";
        }

        public override string GetHelp()
        {
            throw new NotImplementedException();
        }

        public override bool ProcessCommandsExt(MessageWrapper msg)
        {
            if (msg.Content == "ping")
            {
                msg.RawMsg.Channel.SendMessageAsync("Pong!");
                return true;
            }
            else if (msg.Content == "testDialogue")
            {
                //msg.RawMsg.Channel.SendMessageAsync("Starting generic dialogue");
                //addDialogue(msg.RawMsg.Channel.Id, new GenericDialogue());
                addTestDialogue(msg);
                return true;
            }

            return false;
        }

        void addTestDialogue(MessageWrapper msg)
        {
            DialogueStateMachine dialogue = new();

            dialogue.AddTransition(
                "start",
                (string state, SocketMessage msg) => {
                    msg.Channel.SendMessageAsync("Starting the dialogue");
                    return "test1";
                }
            );

            dialogue.AddTransition(
                "test1",
                (string state, SocketMessage msg) => {
                    msg.Channel.SendMessageAsync("State test1");
                    return "test2";
                }
            );

            dialogue.AddTransition(
                "test2",
                (string state, SocketMessage msg) => {
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
                (string state, SocketMessage msg) => {
                    msg.Channel.SendMessageAsync("State test3, terminating Dialogue");
                    return "final";
                }
            );

            dialogue.Update(msg.RawMsg);
            addDialogue(msg.RawMsg.Channel.Id, dialogue);
        }
    }
}
