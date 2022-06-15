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
            _replyStorage.TryAdd(426274169152471041, new( new List<Reply> { new Reply("uncommon message", "hah", ReplyMatchCondition.StartsWith) }, new() ));
        }

        public override string GetHelp()
        {
            throw new NotImplementedException();
        }

        public override bool ProcessCommandsExt(MessageWrapper msg)
        {
            if (msg.Content == "add")
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This will start the dialogue that will guide you through adding your own reply.");
                addDialogue(msg.RawMsg.Channel.Id, new GenericDialogue());
                return true;
            }
            else if (msg.Content.StartsWith("remove "))
            {
                msg.BumpOffset(7);
                if (Guid.TryParse(msg.Content, out var replyId))
                {
                    var replies = _replyStorage[Utils.GetGuild(msg.RawMsg).Id];

                    lock(replies.Item2)
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
            else if (msg.Content == "modify")
            {
                msg.RawMsg.Channel.SendMessageAsync("[WIP] This will start the dialogue that will guide you through modifying a reply.");
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
                // Not ideal! TODO
                lock (replies.Item2)
                {
                    foreach (var reply in replies.Item1)
                    {
                        if (reply.Process(msg.RawMsg))
                            return true;
                    }
                }
            }

            return false;
        }

        ConcurrentDictionary<ulong, ValueTuple<List<Reply>, object>> _replyStorage = new();
    }
}
