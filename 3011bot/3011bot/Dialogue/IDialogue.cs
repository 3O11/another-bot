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
        DialogueStatus Update(SocketMessage msg);
    }
}
