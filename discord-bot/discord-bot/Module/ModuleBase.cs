using System;
using System.Collections.Concurrent;
using System.Text;

namespace bot
{
    internal abstract class ModuleBase : IModule
    {
        public string Keyword { get; protected set; } = "";

        public bool ProcessDialogues(MessageWrapper msg)
        {
            var dialogueId = new ValueTuple<ulong, ulong>(msg.RawMsg.Channel.Id, msg.RawMsg.Author.Id);
            if (_activeDialogues.TryGetValue(dialogueId, out var dialogue))
            {
                var status = dialogue.Update(msg.RawMsg);
                if (status == DialogueStatus.Finished || status == DialogueStatus.Error)
                {
                    _activeDialogues.TryRemove(dialogueId, out var _);
                }

                return true;
            }

            return false;
        }

        public void ProcessCommand(MessageWrapper msg)
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
                    return;
                }

                string commandKeyword = Utils.ExtractFirstKeyword(msg.Content);
                if (_commands.TryGetValue(commandKeyword, out var command))
                {
                    msg.BumpOffset(commandKeyword.Length + (commandKeyword.Length == msg.Content.Length ? 0 : 1));
                    command.Execute(msg);
                }
                else if (commandKeyword == "")
                {
                    msg.RawMsg.Channel.SendMessageAsync("Missing command keyword.");
                }
                else
                {
                    msg.RawMsg.Channel.SendMessageAsync("This module does not contain this command.");
                }
            };
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

        public void AddDialogue(MessageWrapper msg, IDialogue dialogue)
        {
            _activeDialogues[new(msg.RawMsg.Channel.Id, msg.RawMsg.Author.Id)] = dialogue;
        }

        protected string _moduleDescription = "";
        // First key ulong is channelId, second key ulong is userId
        private ConcurrentDictionary<ValueTuple<ulong, ulong>, IDialogue> _activeDialogues = new();
        private ConcurrentDictionary<string, ICommand> _commands = new();
    }
}
