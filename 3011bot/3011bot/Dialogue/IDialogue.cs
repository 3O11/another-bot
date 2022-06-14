using Discord.WebSocket;

namespace bot
{
    interface IDialogue
    {
        bool Update(SocketMessage msg);
    }
}
