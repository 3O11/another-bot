using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public abstract bool Execute(MessageWrapper msg);

        protected ReplyModule _replyModule;
    }

    internal class AddReplyCommand : ReplyCommand
    {
        private static string helpText = "[WIP]";

        public AddReplyCommand(ReplyModule replyModule)
            : base("add", helpText, replyModule)
        { }

        public override bool Execute(MessageWrapper msg)
        {
            var dialogue = ReplyAddDialogue.MakeDialogue(_replyModule);
            dialogue.Update(msg.RawMsg);
            _replyModule.AddDialogue(msg.RawMsg.Channel.Id, dialogue);
            return true;
        }
    }

    internal class RemoveReplyCommand : ReplyCommand
    {
        private static string helpText = "[WIP]";

        public RemoveReplyCommand(ReplyModule replyModule)
            : base("remove", helpText, replyModule)
        { }

        public override bool Execute(MessageWrapper msg)
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

            return true;
        }
    }

    internal class ModifyReplyCommand : ReplyCommand
    {
        private static string helpText = "[WIP]";

        public ModifyReplyCommand(ReplyModule replyModule)
            : base("modify", helpText, replyModule)
        { }

        public override bool Execute(MessageWrapper msg)
        {
            var dialogue = ReplyModifyDialogue.MakeDialogue(_replyModule);
            dialogue.Update(msg.RawMsg);
            _replyModule.AddDialogue(msg.RawMsg.Channel.Id, dialogue);
            return true;
        }
    }

    internal class ListReplyCommand : ReplyCommand
    {
        private static string helpText = "[WIP]";

        public ListReplyCommand(ReplyModule replyModule)
            : base("list", helpText, replyModule)
        { }

        public override bool Execute(MessageWrapper msg)
        {
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
                serializedIds.AppendLine("(This command lists only reply IDs, for more information, use `3022 reply info <Reply ID>`)");

                msg.RawMsg.Channel.SendMessageAsync(serializedIds.ToString());
            }

            return true;
        }
    }

    internal class InfoReplyCommand : ReplyCommand
    {
        private static string helpText = "[WIP]";

        public InfoReplyCommand(ReplyModule replyModule)
            : base("info", helpText, replyModule)
        { }

        public override bool Execute(MessageWrapper msg)
        {
            if (!Guid.TryParse(msg.Content, out var guid))
            {
                msg.RawMsg.Channel.SendMessageAsync("The argument of this command must be a valid Reply ID.");
            }
            else if (_replyModule.TryGetReply(Utils.GetGuild(msg.RawMsg).Id, guid, out var reply))
            {
                msg.RawMsg.Channel.SendMessageAsync(reply.ToString());
            }
            else
            {
                msg.RawMsg.Channel.SendMessageAsync("There is no reply with this ID.");
            }

            return true;
        }
    }
}
