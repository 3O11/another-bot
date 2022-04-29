using System;
using Discord.WebSocket;

namespace bot
{
    internal class MessageWrapper
    {
        public MessageWrapper(SocketMessage message, int initialOffset = 0)
        {
            RawMsg = message;
            _offset = Math.Min(RawMsg.Content.Length, initialOffset);
        }

        public void BumpOffset(int amount = 1)
        {
            _offset = Math.Min(RawMsg.Content.Length, _offset + amount);
        }

        int _offset = 0;

        public SocketMessage RawMsg { get; init; }
        public string Content { get => RawMsg.Content.Substring(_offset); }
    }
}
