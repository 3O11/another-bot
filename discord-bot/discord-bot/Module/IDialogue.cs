using Discord.WebSocket;

namespace bot
{
    internal enum DialogueStatus
    {
        Continue,
        Finished,
        Error
    }

    interface IDialogue
    {
        void PingUser(ulong userId);
        void AppendResponse(string response);
        DialogueStatus Update(SocketMessage msg);
    }
}
