using System;
using Discord.WebSocket;

namespace bot
{
    internal class MessageWrapper
    {
        public MessageWrapper(SocketMessage message, int initialOffset = 0)
        {
            RawMsg = message;
            Offset = Math.Min(RawMsg.Content.Length, initialOffset);
        }

        public void BumpOffset(int amount = 1)
        {
            Offset = Math.Min(RawMsg.Content.Length, Offset + amount);
        }

        public bool IsRaw()
        {
            return Offset == 0;
        }

        public int Offset { get; private set; }

        public SocketMessage RawMsg { get; init; }
        public string Content { get => RawMsg.Content.Substring(Offset); }
    }
}
