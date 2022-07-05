using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;
using Discord.WebSocket;

namespace bot
{
    internal class RepliesModule : DialogueModuleBase
    {
        public RepliesModule()
        {
            Name = "replies";
        }

        public override string GetHelp()
        {
            throw new NotImplementedException();
        }

        public override bool ProcessCommandsExt(MessageWrapper msg)
        {
            if (msg.Content == "add")
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This dialogue is not finished, it might not behave exactly as expected.");
                startAddingDialogue(msg);
                return true;
            }
            else if (msg.Content == "modify")
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This dialogue is not finished, it might not behave exactly as expected.");
                startModifyingDialogue(msg);
                return true;
            }
            else if (msg.Content == "list")
            {
                var replies = _replyStorage[Utils.GetGuild(msg.RawMsg).Id];

                StringBuilder serializedReplies = new();

                lock (replies.Item2)
                {
                    foreach(var reply in replies.Item1)
                    {
                        serializedReplies.Append(reply.Id.ToString() + "\n");
                    }
                }

                if (serializedReplies.Length == 0)
                {
                    msg.RawMsg.Channel.SendMessageAsync("No replies registered.");
                }
                else
                {
                    msg.RawMsg.Channel.SendMessageAsync(serializedReplies.ToString());
                }
                return true;
            }
            else if (msg.Content.StartsWith("remove "))
            {
                msg.BumpOffset(7);
                if (Guid.TryParse(msg.Content, out var replyId))
                {
                    var replies = _replyStorage[Utils.GetGuild(msg.RawMsg).Id];

                    lock (replies.Item2)
                    {
                        var temp = replies.Item1.Count;
                        replies.Item1.RemoveAll(reply => reply.Id == replyId);

                        if (temp != replies.Item1.Count)
                        {
                            msg.RawMsg.Channel.SendMessageAsync("Reply removed successfully.");
                        }
                        else
                        {
                            msg.RawMsg.Channel.SendMessageAsync("This reply didn't exist.");
                        }
                    }
                }
                else
                {
                    msg.RawMsg.Channel.SendMessageAsync("Invalid reply ID, please check if you have provided an ID from the list of current replies.");
                }

                return true;
            }
            else if (msg.Content.StartsWith("info "))
            {
                msg.BumpOffset(5);
                if (Guid.TryParse(msg.Content, out var replyId))
                {
                    var replies = _replyStorage[Utils.GetGuild(msg.RawMsg).Id];

                    lock (replies.Item2)
                    {
                        var reply = replies.Item1.FindAll(reply => reply.Id == replyId);

                        if (reply.Count == 1)
                        {
                            msg.RawMsg.Channel.SendMessageAsync("```\n" + reply[0].ToString() + "```");
                        }
                        else
                        {
                            msg.RawMsg.Channel.SendMessageAsync("This reply doesn't exist.");
                        }
                    }
                }
                else
                {
                    msg.RawMsg.Channel.SendMessageAsync("Invalid reply ID, please check if you have provided an ID from the list of current replies.");
                }
                return true;
            }

            return false;
        }

        public override bool ProcessTriggers(MessageWrapper msg)
        {
            if (_replyStorage.TryGetValue(Utils.GetGuild(msg.RawMsg).Id, out var replies))
            {
                lock (replies.Item2)
                {
                    // Not ideal, I'll have to fix this somehow
                    foreach (var reply in replies.Item1)
                    {
                        if (reply.Process(msg.RawMsg))
                            return true;
                    }
                }
            }

            return false;
        }

        public void AddReply(ulong guildId, Reply reply)
        {
            if (_replyStorage.TryGetValue(guildId, out var replies))
            {
                lock(replies.Item2)
                {
                    replies.Item1.Add(reply);
                }
            }
            else
            {
                _replyStorage.TryAdd(guildId, new(new List<Reply> { reply }, new()));
            }
        }

        void startAddingDialogue(MessageWrapper msg)
        {
            var dialogue = ReplyAddDialogue.MakeDialogue(this);
            dialogue.Update(msg.RawMsg);
            addDialogue(msg.RawMsg.Channel.Id, dialogue);
        }

        void startModifyingDialogue(MessageWrapper msg)
        {
            var dialogue = ReplyModifyDialogue.MakeDialogue();
            dialogue.Update(msg.RawMsg);
            addDialogue(msg.RawMsg.Channel.Id, dialogue);
        }

        ConcurrentDictionary<ulong, ValueTuple<List<Reply>, object>> _replyStorage = new();
    }

    class ReplyAddDialogue : DialogueStateMachine
    {
        ReplyAddDialogue(RepliesModule repliesModule)
        {
            reply = "";
            trigger = "";
            replyModule = repliesModule;
        }

        public static ReplyAddDialogue MakeDialogue(RepliesModule module)
        {
            // I know it's not good practice to use single letter variable names,
            // but I think that it improves readability in this case.
            ReplyAddDialogue d = new(module);

            d.AddTransition(
                "start",
                (string state, SocketMessage msg) =>
                {
                    msg.Channel.SendMessageAsync("Specify trigger");
                    return "trigger";
                }
            );

            d.AddTransition(
                "trigger",
                (string state, SocketMessage msg) =>
                {
                    d.trigger = msg.Content;

                    msg.Channel.SendMessageAsync("Specify match type [any, full, startsWith, endsWith]");
                    return "matchType";
                }
            );

            d.AddTransition(
                "matchType",
                (string state, SocketMessage msg) =>
                {
                    switch (msg.Content)
                    {
                        case "any":
                            d.matchCondition = ReplyMatchCondition.Any;
                            break;
                        case "full":
                            d.matchCondition = ReplyMatchCondition.Full;
                            break;
                        case "startsWith":
                            d.matchCondition = ReplyMatchCondition.StartsWith;
                            break;
                        case "endsWith":
                            d.matchCondition = ReplyMatchCondition.EndsWith;
                            break;
                        default:
                            msg.Channel.SendMessageAsync("The match type was not recognized, please try again.");
                            return "matchType";
                    }

                    msg.Channel.SendMessageAsync("Specify reply body");
                    return "reply";
                }
            );

            d.AddTransition(
                "reply",
                (string state, SocketMessage msg) =>
                {
                    d.reply = msg.Content;

                    msg.Channel.SendMessageAsync("Specify target users via mentions or IDs [separate each ID with any sequence of non-numeric characters, or you can use `everyone`]");
                    return "users";
                }
            );

            d.AddTransition(
                "users",
                (string state, SocketMessage msg) =>
                {
                    if (msg.Content != "everyone")
                    {
                        d.userIds = Utils.ExtractIds(msg.Content);
                        if (d.userIds == null)
                        {
                            msg.Channel.SendMessageAsync("No valid IDs have been found, please try again or specify `everyone` explicitly");
                            return "users";
                        }
                    }

                    msg.Channel.SendMessageAsync("Specify target channels [or use `anywhere`]");
                    return "channels";
                }
            );

            d.AddTransition(
                "channels",
                (string state, SocketMessage msg) =>
                {
                    if (msg.Content != "anywhere")
                    {
                        d.channelIds = Utils.ExtractIds(msg.Content);
                        if (d.channelIds == null)
                        {
                            msg.Channel.SendMessageAsync("No valid IDs have been found, please try again or specify `anywhere` explicitly");
                            return "channels";
                        }
                    }

                    msg.Channel.SendMessageAsync("Adding reply ...");

                    d.replyModule.AddReply(
                        Utils.GetGuild(msg).Id, 
                        new Reply(
                            d.trigger,
                            d.reply,
                            d.matchCondition,
                            d.channelIds != null ? new(d.channelIds) : null,
                            d.userIds != null ? new(d.userIds) : null
                        )
                    );

                    msg.Channel.SendMessageAsync("Closing dialogue");

                    return "final";
                }
            );

            return d;
        }

        string trigger { get; set; }
        string reply { get; set; }
        ReplyMatchCondition matchCondition { get; set; }
        List<ulong>? userIds { get; set; }
        List<ulong>? channelIds { get; set; }
        RepliesModule replyModule { get; init; }
    }

    class ReplyModifyDialogue : DialogueStateMachine
    {
        ReplyModifyDialogue()
        {

        }

        public static ReplyModifyDialogue MakeDialogue()
        {
            ReplyModifyDialogue d = new();

            return d;
        }
    }
}
