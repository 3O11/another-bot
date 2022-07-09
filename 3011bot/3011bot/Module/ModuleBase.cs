using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace bot
{
    internal abstract class ModuleBase : IModule
    {
        public string Keyword { get; protected set; } = "";

        public bool ProcessDialogues(MessageWrapper msg)
        {
            if (_activeDialogues.TryGetValue(msg.RawMsg.Channel.Id, out var dialogue))
            {
                if (dialogue.Update(msg.RawMsg) == DialogueStatus.Finished)
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
            if (content.StartsWith(Keyword))
            {
                // This looks a bit weird, but I haven't figured out how to
                // make it better yet.
                if (content.Length == Keyword.Length)
                {
                    msg.BumpOffset(Keyword.Length);
                }
                else if (content[Keyword.Length] == ' ')
                {
                    msg.BumpOffset(Keyword.Length + 1);
                }
                else
                {
                    return false;
                }

                int spacePos = msg.Content.IndexOf(' ');
                int offset = spacePos < 0 ? msg.Content.Length : spacePos;
                string commandKeyword = msg.Content.Substring(0, offset);
                if (_commands.TryGetValue(commandKeyword, out var command))
                {
                    msg.BumpOffset(offset + (spacePos < 0 ? 0 : 1));
                    if (command.Execute(msg)) return true;
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

        public void AddCommand(ICommand command)
        {
            _commands[command.Keyword] = command;
        }

        public virtual string GetHelpString(string commandKeyword = "")
        {
            if (commandKeyword == "")
            {
                StringBuilder commandList = new();

                commandList.AppendLine("Module description:");
                commandList.AppendLine(_moduleDescription);
                commandList.AppendLine("");
                commandList.AppendLine("Available commands:");
                commandList.AppendLine("```");
                foreach (var commandName in _commands.Keys)
                {
                    commandList.AppendLine(commandName);
                }
                commandList.AppendLine("```");

                return commandList.ToString();
            }
            else if (_commands.TryGetValue(commandKeyword, out var command))
            {
                return command.HelpText;
            }
            else
            {
                return "This module does not contain this command.";
            }
        }

        public virtual string GetCommandNames()
        {
            StringBuilder names = new();

            foreach (var commandName in _commands.Keys)
            {
                names.AppendLine(commandName);
            }

            return names.ToString();
        }

        // This is currently in the process of being removed, it will be removed fully soon.
        public virtual bool ProcessCommandsExt(MessageWrapper msg)
        {
            return false;
        }

        public void AddDialogue(ulong channelId, IDialogue dialogue)
        {
            _activeDialogues[channelId] = dialogue;
        }

        protected string _moduleDescription = "";
        private ConcurrentDictionary<ulong, IDialogue> _activeDialogues = new();
        private ConcurrentDictionary<string, ICommand> _commands = new();
    }
}
