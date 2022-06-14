using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace bot
{
    internal abstract class DialogueModuleBase : IModule
    {
        public string Name { get; protected set; } = "";

        public bool ProcessDialogues(MessageWrapper msg)
        {
            if (_activeDialogues.TryGetValue(msg.RawMsg.Channel.Id, out var dialogue))
            {
                if (dialogue.Update(msg.RawMsg))
                {
                    _activeDialogues.TryRemove(msg.RawMsg.Channel.Id, out var _);
                }

                return true;
            }

            return false;
        }

        public bool ProcessCommands(MessageWrapper msg)
        {
            var content = msg.Content;
            if (content.StartsWith(Name))
            {
                // This looks a bit weird, but I haven't figured out how to
                // make it better yet.
                if (content.Length == Name.Length)
                {
                    msg.BumpOffset(Name.Length);
                }
                else if (content[Name.Length] == ' ')
                {
                    msg.BumpOffset(Name.Length + 1);
                }
                else
                {
                    return false;
                }

                if (!ProcessCommandsExt(msg))
                {
                    msg.RawMsg.Channel.SendMessageAsync("Unknown command for this module.");
                }

                return true;
            }

            return false;
        }

        public virtual bool ProcessTriggers(MessageWrapper msg)
        {
            return false;
        }

        public abstract string GetHelp();
        public abstract bool ProcessCommandsExt(MessageWrapper msg);

        protected void addDialogue(ulong channelId, IDialogue dialogue)
        {
            _activeDialogues[channelId] = dialogue;
        }

        private ConcurrentDictionary<ulong, IDialogue> _activeDialogues = new();
    }
}
