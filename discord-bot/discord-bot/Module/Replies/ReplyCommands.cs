using System;
using System.Collections.Generic;
using System.Text;

namespace bot
{
    internal abstract class ReplyCommand : ICommand
    {
        public ReplyCommand(string keyword, string helpText, ReplyModule replyModule)
        {
            Keyword = keyword;
            HelpText = helpText;
            _replyModule = replyModule;
        }

        public string Keyword { get; init; }
        public string HelpText { get; init; }
        public abstract void Execute(MessageWrapper msg);

        protected ReplyModule _replyModule;
    }

    internal class AddReplyCommand : ReplyCommand
    {
        private static string helpText =
            "Usage: <botname> <reply> add\n" +
            "\n" +
            "Takes no parameters.\n" +
            "Starts a dialogue with which you can set up a new reply.";

        public AddReplyCommand(ReplyModule replyModule)
            : base("add", helpText, replyModule)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                var dialogue = ReplyAddDialogue.MakeDialogue(_replyModule);
                dialogue.Update(msg.RawMsg);
                _replyModule.AddDialogue(msg.RawMsg.Channel.Id, dialogue);
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync("This command takes no arguments.");
            }
        }
    }

    internal class RemoveReplyCommand : ReplyCommand
    {
        private static string helpText =
            "Usage: <botname> <reply> remove <Reply ID>\n" +
            "\n" +
            "Takes a single required parameter in the form of a GUID.\n" +
            "Permanently removes the reply specified by the ID.";

        public RemoveReplyCommand(ReplyModule replyModule)
            : base("remove", helpText, replyModule)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if (!Guid.TryParse(msg.Content, out var guid))
            {
                msg.RawMsg.Channel.SendMessageAsync("The argument of this command must be a valid Reply ID.");
            }
            else if (_replyModule.RemoveReply(Utils.GetGuild(msg.RawMsg).Id, guid))
            {
                msg.RawMsg.Channel.SendMessageAsync("The reply was removed succesfully.");
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync("There is no reply with this ID.");
            }
        }
    }

    internal class ModifyReplyCommand : ReplyCommand
    {
        private static string helpText =
            "Usage: <botname> <reply> modify\n" +
            "\n" +
            "Takes no parameters.\n" +
            "Starts a dialogue that will guide through modifying an existing reply.";

        public ModifyReplyCommand(ReplyModule replyModule)
            : base("modify", helpText, replyModule)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if (msg.Content == "")
            {
                var dialogue = ReplyModifyDialogue.MakeDialogue(_replyModule);
                dialogue.Update(msg.RawMsg);
                _replyModule.AddDialogue(msg.RawMsg.Channel.Id, dialogue);
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync("This command takes no arguments.");
            }
        }
    }

    internal class ListReplyCommand : ReplyCommand
    {
        private static string helpText =
            "Usage: <botname> <reply> list\n" +
            "\n" +
            "Takes no parameters.\n" +
            "Lists all IDs of replies that have been registered on this server.";

        public ListReplyCommand(ReplyModule replyModule)
            : base("list", helpText, replyModule)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if (msg.Content != "")
            {
                msg.RawMsg.Channel.SendMessageAsync("This command takes no arguments.");
            }

            List<Guid> replyIds = _replyModule.GetReplyIds(Utils.GetGuild(msg.RawMsg).Id);

            if (replyIds.Count == 0)
            {
                msg.RawMsg.Channel.SendMessageAsync("No replies registered.");
            }
            else
            {
                StringBuilder serializedIds = new();

                serializedIds.AppendLine("Replies registered on this server:");
                serializedIds.AppendLine("```");
                foreach (var replyId in replyIds)
                {
                    serializedIds.AppendLine(replyId.ToString());
                }
                serializedIds.AppendLine("```");
                serializedIds.AppendLine("(This command lists only reply IDs, for more information, use `<botname> <reply> info <Reply ID>`)");

                msg.RawMsg.Channel.SendMessageAsync(serializedIds.ToString());
            }
        }
    }

    internal class InfoReplyCommand : ReplyCommand
    {
        private static string helpText =
            "Usage: <botname> <reply> info <Reply ID>\n" +
            "\n" +
            "Takes a single required parameter in the form of a GUID.\n" +
            "Displays all information about an existing reply.";

        public InfoReplyCommand(ReplyModule replyModule)
            : base("info", helpText, replyModule)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if (!Guid.TryParse(msg.Content, out var guid))
            {
                msg.RawMsg.Channel.SendMessageAsync("The argument of this command must be a valid Reply ID.");
            }
            else if (_replyModule.TryGetReply(Utils.GetGuild(msg.RawMsg).Id, guid, out var reply) && reply != null)
            {
                msg.RawMsg.Channel.SendMessageAsync(reply.ToString());
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync("There is no reply with this ID.");
            }

        }
    }
}
