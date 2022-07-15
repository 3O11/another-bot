using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace bot
{
    class ReplyAddDialogue : DialogueBase
    {
        ReplyAddDialogue(ReplyModule module)
        {
            reply = "";
            trigger = "";
            replyModule = module;
        }

        public static ReplyAddDialogue MakeDialogue(ReplyModule module)
        {
            // I know it's not good practice to use single letter variable names,
            // but I think that it improves readability in this case.
            ReplyAddDialogue d = new(module);

            d.AddTransition(
                "start",
                (string state, SocketMessage msg) =>
                {
                    d._outBuffer = "Specify trigger, type `everything` to make the bot respond to everything.";
                    return "trigger";
                }
            );

            d.AddTransition(
                "trigger",
                (string state, SocketMessage msg) =>
                {
                    if (msg.Content == "everything")
                    {
                        d.trigger = null;
                    }
                    else
                    {
                        d.trigger = msg.Content;
                    }

                    d._outBuffer = "Specify match type [any, full, startsWith, endsWith]";
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
                        d._outBuffer = "The match type was not recognized, please try again.";
                        return "matchType";
                    }

                    d._outBuffer = "Specify reply body";
                    return "reply";
                }
            );

            d.AddTransition(
                "reply",
                (string state, SocketMessage msg) =>
                {
                    d.reply = msg.Content;

                    d._outBuffer = "Specify target users via mentions or IDs [separate each ID with any sequence of non-numeric characters, or you can use `everyone`]";
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
                            d._outBuffer = "No valid IDs have been found, please try again or specify `everyone` explicitly";
                            return "users";
                        }
                    }

                    d._outBuffer = "Specify target channels [or use `anywhere`]";
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
                            d._outBuffer = "No valid IDs have been found, please try again or specify `anywhere` explicitly";
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

        string? trigger { get; set; }
        string reply { get; set; }
        ReplyMatchCondition matchCondition { get; set; }
        List<ulong>? userIds { get; set; }
        List<ulong>? channelIds { get; set; }
        ReplyModule replyModule { get; init; }
    }

    class ReplyModifyDialogue : DialogueBase
    {
        ReplyModifyDialogue(ReplyModule module)
        {
            replyModule = module;
        }

        public static ReplyModifyDialogue MakeDialogue(ReplyModule module)
        {
            ReplyModifyDialogue d = new(module);

            d.AddTransition(
                "start",
                (string state, SocketMessage msg) =>
                {
                    msg.Channel.SendMessageAsync("Specify the ID of the reply to be modified");
                    return "replyId";
                }
            );

            d.AddTransition(
                "replyId",
                (string state, SocketMessage msg) =>
                {
                    if (Guid.TryParse(msg.Content, out var replyId))
                    {
                        if (d.replyModule.TryGetReply(Utils.GetGuild(msg).Id, replyId, out var reply) && reply != null)
                        {
                            d.reply = reply;
                            d._outBuffer = "Reply selected, specify which value you'd like to modify [trigger, reply, matchCondition, users, channels]";
                            return "modValue";
                        }
                        else
                        {
                            d._outBuffer = "There is no message with this ID, please try another.";
                            return "replyId";
                        }
                    }
                    else
                    {
                        d._outBuffer = "That is not a valid ID, please try again";
                        return "replyId";
                    }
                }
            );

            d.AddTransition(
                "modValue",
                (string state, SocketMessage msg) =>
                {
                    string[] keywords = new string[] { "trigger", "reply", "matchCondition", "users", "channels" };
                    if (keywords.Contains(msg.Content))
                    {
                        d._outBuffer = msg.Content + " selected, specify the new value";
                        return msg.Content;
                    }
                    else
                    {
                        d._outBuffer = "Invalid value keyword, please try again";
                        return "modValue";
                    }
                }
            );

            d.AddTransition(
                "trigger",
                (string state, SocketMessage msg) =>
                {
                    d.reply.SetTrigger(msg.Content);

                    d._outBuffer = "Change another value? [y/N]";
                    return "repeat";
                }
            );

            d.AddTransition(
                "reply",
                (string state, SocketMessage msg) =>
                {
                    d.reply.SetReply(msg.Content);

                    d._outBuffer = "Change another value? [y/N]";
                    return "repeat";
                }
            );

            d.AddTransition(
                "matchCondition",
                (string state, SocketMessage msg) =>
                {
                    switch (msg.Content)
                    {
                        case "any":
                            d.reply.SetMatchCondition(ReplyMatchCondition.Any);
                            break;
                        case "full":
                            d.reply.SetMatchCondition(ReplyMatchCondition.Full);
                            break;
                        case "startsWith":
                            d.reply.SetMatchCondition(ReplyMatchCondition.StartsWith);
                            break;
                        case "endsWith":
                            d.reply.SetMatchCondition(ReplyMatchCondition.EndsWith);
                            break;
                        default:
                            d._outBuffer = "The match type was not recognized, please try again.";
                            return "matchCondition";
                    }

                    d._outBuffer = "Change another value? [y/N]";
                    return "repeat";
                }
            );

            d.AddTransition(
                "users",
                (string state, SocketMessage msg) =>
                {
                    if (msg.Content != "everyone")
                    {
                        var userIds = Utils.ExtractIds(msg.Content);
                        if (userIds == null)
                        {
                            d._outBuffer = "No valid IDs have been found, please try again or specify `everyone` explicitly";
                            return "users";
                        }
                        d.reply.SetUsers(userIds);
                    }
                    else
                    {
                        d.reply.SetUsers(null);
                    }

                    d._outBuffer = "Change another value? [y/N]";
                    return "repeat";
                }
            );

            d.AddTransition(
                "channels",
                (string state, SocketMessage msg) =>
                {
                    if (msg.Content != "anywhere")
                    {
                        var channelIds = Utils.ExtractIds(msg.Content);
                        if (channelIds == null)
                        {
                            d._outBuffer = "No valid IDs have been found, please try again or specify `anywhere` explicitly";
                            return "channels";
                        }
                        d.reply.SetChannels(channelIds);
                    }
                    else
                    {
                        d.reply.SetChannels(null);
                    }

                    d._outBuffer = "Change another value? [y/N]";
                    return "repeat";
                }
            );

            d.AddTransition(
                "repeat",
                (string state, SocketMessage msg) =>
                {
                    string[] keywords = new string[] { "y", "Y", "yes", "Yes" };
                    if (keywords.Contains(msg.Content))
                    {
                        d._outBuffer = "Specify which value you'd like to modify [trigger, reply, matchCondition, userIds, channelIds]";
                        return "modValue";
                    }
                    else
                    {
                        d._outBuffer = "Terminating dialogue";
                        return "final";
                    }
                }
            );

            return d;
        }

        Reply reply { get; set; }
        ReplyModule replyModule { get; set; }
    }
}
