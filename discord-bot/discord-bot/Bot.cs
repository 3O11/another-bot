using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;
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

            var config = new DiscordSocketConfig();
            config.GatewayIntents = GatewayIntents.AllUnprivileged 
                                  | GatewayIntents.MessageContent;

            _client = new DiscordSocketClient(config);

            _client.Log += Log;
            _client.MessageReceived += OnMessage;
        }

        public void AddCommand(ICommand command)
        {
            _commands[command.Keyword] = command;
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

        // This feels a bit out of place here, I'll have to think about this later.
        public string GetHelpString(string keyword)
        {
            if (keyword == "")
            {
                StringBuilder message = new();

                message.AppendLine("**Bot commands:**");
                foreach (var commandName in _commands.Keys)
                {
                    message.AppendLine(commandName);
                }
                message.AppendLine("");

                message.AppendLine("**Modules:**");
                foreach (var module in _modules.Values)
                {
                    message.AppendLine("Name: `" + module.Keyword + "`");
                    message.AppendLine("Commands:");
                    string commands = module.GetCommandNames();
                    // A bad hack to make the discord code block parser work properly.
                    message.AppendLine("```\n" + (commands == "" ? " " : commands) + "```");
                }

                return message.ToString();
            }

            string firstKeyword = Utils.ExtractFirstKeyword(keyword);
            if (_commands.TryGetValue(keyword, out var command))
            {
                return command.HelpText;
            }
            else if (_modules.TryGetValue(firstKeyword, out var module))
            {
                if (firstKeyword.Length == keyword.Length)
                {
                    return module.GetHelpString();
                }
                else
                {
                    return module.GetHelpString(keyword.Substring(firstKeyword.Length + 1));
                }
            }
            else
            {
                return "There is no command or module by this name.";
            }
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

                if (msg.Channel is ISocketPrivateChannel)
                    return;

                // Evaluate message
                var wrappedMsg = new MessageWrapper(msg, msg.Content.StartsWith(Name + " ") ? Name.Length + 1 : 0);
                foreach (var module in _modules.Values)
                {
                    if (module.ProcessDialogues(wrappedMsg)) return;
                }

                if (msg.Content.StartsWith(Name))
                {
                    string keyword = Utils.ExtractFirstKeyword(msg.Content, wrappedMsg.Offset);
                    if (_commands.TryGetValue(keyword, out var command))
                    {
                        wrappedMsg.BumpOffset(keyword.Length + (msg.Content.Length == keyword.Length ? 0 : 1));
                        command.Execute(wrappedMsg);
                        return;
                    }
                    if (_modules.TryGetValue(keyword, out var commModule))
                    {
                        commModule.ProcessCommand(wrappedMsg);
                        return;
                    }
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
        private DiscordSocketClient _client;
        private ConcurrentDictionary<string, ICommand> _commands = new();
        private ConcurrentDictionary<string, IModule> _modules = new();
    }
}
