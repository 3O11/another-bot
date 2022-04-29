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
        public Bot(string name)
        {
            _name = name;
            _client = new DiscordSocketClient();
            _modules = new ConcurrentDictionary<string, IModule>();

            _client.Log += Log;
            _client.MessageReceived += OnMessage;
        }

        public void RegisterModule(IModule module)
        {
            _modules[module.Name] = module;
        }

        public async void Start()
        {
            string token = "";
            await _client.LoginAsync(TokenType.Bot, token);
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
                if (msg.Author.IsBot) return;

                // Evaluate dialogues

                // Evaluate command
                if (msg.Content.StartsWith(_name + " "))
                {
                    var msgWrapper = new MessageWrapper(msg, _name.Length + 1);

                    int nameLength = msgWrapper.Content.IndexOf(' ');
                    string moduleName = nameLength == -1 ? msgWrapper.Content : msgWrapper.Content.Substring(0, nameLength);

                    if (_modules.TryGetValue(moduleName, out IModule? module))
                    {
                        msgWrapper.BumpOffset(module.Name.Length + 1);
                        module.Process(msgWrapper);
                    }
                }
            });
        }

        private string _name;
        private DiscordSocketClient _client;
        private ConcurrentDictionary<string, IModule> _modules;
    }
}
