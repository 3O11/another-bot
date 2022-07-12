using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

// It might be overkill to use Guids here, but it is a more reliable and faster
// method of generating unique IDs than what I'd probably come up with.

namespace bot
{
    internal enum ReplyMatchCondition
    {
        Full,
        Any,
        StartsWith,
        EndsWith
    }

    internal class Reply
    {
        public Reply(string? trigger, string reply, ReplyMatchCondition match, HashSet<ulong>? channelIds = null, HashSet<ulong>? userIds = null)
        {
            Id = Guid.NewGuid();
            _trigger = trigger;
            _reply = reply;
            _condition = match;
            _channels = channelIds != null ? channelIds : new();
            _users = userIds != null ? userIds : new();
        }

        public Reply(Reply other)
        {
            Id = other.Id;
            _trigger = other._trigger;
            _reply = other._reply;
            _condition = other._condition;
            _channels = new HashSet<ulong>(other._channels);
            _users = new HashSet<ulong>(other._users);
        }

        public Reply(ReplyRecord other)
        {
            Id = other.Id;
            _trigger = other.Trigger;
            _reply = other.Reply;
            _condition = other.Condition;
            _channels = other.Channels;
            _users = other.Users;
        }

        public bool Process(SocketMessage msg)
        {
            lock(_replyLock)
            {
                bool shouldReply = true;

                if (_trigger != null)
                {
                    switch (_condition)
                    {
                        case ReplyMatchCondition.Full:
                            shouldReply = (msg.Content == _trigger);
                            break;
                        case ReplyMatchCondition.Any:
                            shouldReply = msg.Content.Contains(_trigger);
                            break;
                        case ReplyMatchCondition.StartsWith:
                            shouldReply = msg.Content.StartsWith(_trigger);
                            break;
                        case ReplyMatchCondition.EndsWith:
                            shouldReply = msg.Content.EndsWith(_trigger);
                            break;
                        default:
                            shouldReply = false;
                            break;
                    }
                }

                shouldReply &= _users.Count > 0 ? _users.Contains(msg.Author.Id) : true;
                shouldReply &= _channels.Count > 0 ? _channels.Contains(msg.Channel.Id) : true;

                if (shouldReply)
                {
                    msg.Channel.SendMessageAsync(_reply);
                    return true;
                }

                return false;
            }
        }

        public void SetReply(string reply)
        {
            _reply = reply;
        }

        public void SetTrigger(string trigger)
        {
            _trigger = trigger;
        }

        public void SetMatchCondition(ReplyMatchCondition condition)
        {
            _condition = condition;
        }

        public void SetChannels(List<ulong>? channels)
        {
            _channels = channels != null ? new(channels) : new();
        }

        public bool AddChannel(ulong channelId)
        {
            lock(_replyLock)
            {
                return _channels.Add(channelId);
            }
        }

        public bool RemoveChannel(ulong channelId)
        {
            lock (_replyLock)
            {
                return _channels.Remove(channelId);
            }
        }

        public void SetUsers(List<ulong>? users)
        {
            _users = users != null ? new(users) : new();
        }

        public bool AddUser(ulong userId)
        {
            lock (_replyLock)
            {
                return _users.Add(userId);
            }
        }

        public bool RemoveUser(ulong userId)
        {
            lock (_replyLock)
            {
                return _users.Remove(userId);
            }
        }

        public override string ToString()
        {
            StringBuilder str = new();
            lock(_replyLock)
            {
                str.Append("**Trigger:**\n");
                if (_trigger == null)
                {
                    str.Append("everything");
                }
                else
                {
                    str.Append(_trigger);
                }
                str.Append("\n\n");
                str.Append("**Reply:**\n");
                str.Append(_reply);
                str.Append("\n\n");
                str.Append("**Reply to** (user IDs)**:**\n");
                if (_users.Count > 0)
                {
                    foreach (var userId in _users) str.Append(userId.ToString() + "\n");
                }
                else
                {
                    str.Append("everyone\n");
                }
                str.Append("\n");
                str.Append("**Reply in** (channel IDs)**:**\n");
                if (_channels.Count > 0)
                {
                    foreach (var channelId in _channels) str.Append(channelId.ToString() + "\n");
                }
                else
                {
                    str.Append("anywhere\n");
                }
                str.Append("\n");
                str.Append("**Match type:**\n" + _condition.ToString() + "\n\n");
                str.Append("**ID:**\n");
                str.Append("`" + Id.ToString() + "`");
                str.Append("\n");
            }

            return str.ToString();
        }

        public ReplyRecord GetRecord()
        {
            lock (_replyLock)
            {
                return new ReplyRecord(Id, _reply, _trigger, _condition, _channels, _users);
            }
        }

        public Guid Id { get; init; }
        private string _reply;
        private string? _trigger;
        private ReplyMatchCondition _condition;
        private HashSet<ulong> _channels;
        private HashSet<ulong> _users;
        private readonly object _replyLock = new();
    }

    internal record class ReplyRecord(Guid Id, string Reply, string? Trigger, ReplyMatchCondition Condition, HashSet<ulong> Channels, HashSet<ulong> Users);
}
