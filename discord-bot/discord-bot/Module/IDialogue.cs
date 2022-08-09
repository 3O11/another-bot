using Discord.WebSocket;

namespace bot
{
    internal enum DialogueStatus
    {
        Continue,
        Finished,
        Error
    }

    /// <summary>
    /// This interface intentionally provides only the bare minimum that is
    /// needed to enable different styles of dialogue implementation.
    /// 
    /// The Update function is called each time the Dialogue should update.
    /// </summary>
    interface IDialogue
    {
        DialogueStatus Update(SocketMessage msg);
    }
}
