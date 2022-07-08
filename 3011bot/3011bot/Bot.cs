using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace bot
{
    internal class Bot
    {
        public Bot(string name, string token)
        {
            Name = name;
            _token = token;

            _client.Log += Log;
            _client.MessageReceived += OnMessage;
        }

        public void AddModule(IModule module)
        {
            _modules[module.Keyword] = module;
        }

        public async void Start()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task OnMessage(SocketMessage msg)
        {
            return Task.Run(() => {
                if (msg.Author.IsBot) 
                    return;

                // Evaluate message
                var wrappedMsg = new MessageWrapper(msg, msg.Content.StartsWith(Name + " ") ? Name.Length + 1 : 0);
                foreach (var module in _modules.Values)
                {
                    if (module.ProcessDialogues(wrappedMsg)) return;
                }

                int spacePos = msg.Content.IndexOf(' ', wrappedMsg.Offset);
                var moduleKeyword = msg.Content.Substring(wrappedMsg.Offset, (spacePos < 0 ? msg.Content.Length : spacePos) - wrappedMsg.Offset);
                if (_modules.TryGetValue(moduleKeyword, out var commModule))
                {
                    commModule.ProcessCommands(wrappedMsg);
                    return;
                }

                foreach (var module in _modules.Values)
                {
                    if (module.ProcessTriggers(wrappedMsg)) return;
                }

                if (!wrappedMsg.IsRaw())
                {
                    msg.Channel.SendMessageAsync("Unknown command");
                }
            });
        }

        public string Name { get; init; }

        private string _token;
        private DiscordSocketClient _client = new();
        private ConcurrentDictionary<string, IModule> _modules = new();
    }
}
