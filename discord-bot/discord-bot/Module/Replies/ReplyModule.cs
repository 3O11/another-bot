using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace bot
{
    internal class ReplyModule : ModuleBase
    {
        ReplyModule(string keyword)
        {
            Keyword = keyword;
        }

        public static ReplyModule MakeModule(string keyword)
        {
            var module = new ReplyModule(keyword);

            module._moduleDescription =
                "This module implements the reply functionality, " +
                "you can make the bot reply to messages, users and in " +
                "channels of your choosing. Use the commands below to " +
                "set up the replies you want.";

            module.AddCommand(new AddReplyCommand(module));
            module.AddCommand(new RemoveReplyCommand(module));
            module.AddCommand(new ModifyReplyCommand(module));
            module.AddCommand(new ListReplyCommand(module));
            module.AddCommand(new InfoReplyCommand(module));
            module.AddCommand(new BackupReplyCommand(module));
            module.AddCommand(new LoadBackupReplyCommand(module));

            return module;
        }

        public override bool ProcessTriggers(MessageWrapper msg)
        {
            if (_replyStorage.TryGetValue(Utils.GetGuild(msg.RawMsg).Id, out var replies))
            {
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

        public bool RemoveReply(ulong guildId, Guid replyId)
        {
            if (!_replyStorage.TryGetValue(guildId, out var replies))
            {
                return false;
            }

            lock (replies.Item2)
            {
                var temp = replies.Item1.Count;
                // I hope that remove won't be called often enough to cause
                // preformance issues here.
                replies.Item1.RemoveAll(reply => reply.Id == replyId);
                return temp != replies.Item1.Count;
            }
        }

        public bool TryGetReply(ulong guildId, Guid replyId, out Reply? reply)
        {
            if(_replyStorage.TryGetValue(guildId, out var replies))
            {
                lock(replies.Item2)
                {
                    reply = replies.Item1.Find(listReply => listReply.Id == replyId);
                    return reply != null;
                }
            }
            else
            {
                reply = null;
                return false;
            }
        }

        public List<Guid> GetReplyIds(ulong guildId)
        {
            List<Guid> replyIds = new();

            if (!_replyStorage.TryGetValue(guildId, out var replies))
            {
                return replyIds;
            }

            lock (replies.Item2)
            {
                foreach (var reply in replies.Item1)
                {
                    replyIds.Add(reply.Id);
                }
            }

            return replyIds;
        }

        public List<ReplyRecord> GetReplyRecords(ulong guildId)
        {
            if (_replyStorage.TryGetValue(guildId, out var replies))
            {
                lock (replies.Item2)
                {
                    return replies.Item1.ConvertAll(reply => reply.GetRecord());
                }
            }
            else
            {
                return new();
            }
        }

        public void SetReplies(ulong guildId, List<ReplyRecord> records)
        {
            if (_replyStorage.TryGetValue(guildId, out var replies))
            {
                lock (replies.Item2)
                {
                    replies.Item1 = records.ConvertAll(replyRecord => new Reply(replyRecord));
                }
            }
            else
            {
                _replyStorage.TryAdd(guildId, new(records.ConvertAll(replyRecord => new Reply(replyRecord)), new()));
            }
        }

        // Using Lists for the actual per-server reply storage is far from ideal.
        // I am not sure if there is a thread-safe collection that would allow me
        // to index by two different keys as easily as a List.
        ConcurrentDictionary<ulong, ValueTuple<List<Reply>, object>> _replyStorage = new();
    }
}
