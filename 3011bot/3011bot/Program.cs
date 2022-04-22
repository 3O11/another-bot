using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace bot
{
	public class Program
	{
		public static Task Main(string[] args) => new Program().MainAsync();

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		private Task UserTyping(Cacheable<IUser, ulong> user, Cacheable<IMessageChannel, ulong> channel)
        {
			if (user.Value.IsBot) return Task.CompletedTask;
			if (channel.Id != 476271238273171458) return Task.CompletedTask;

			channel.Value.SendMessageAsync($"<@{user.Id}>'s typing!");
			return Task.CompletedTask;
		}

		private Task OnMessage(SocketMessage msg)
        {
			if (msg.Author.IsBot) return Task.CompletedTask;
			if (msg.Author.Id == 431514955523817473) return Task.CompletedTask;
			if (msg.Channel.Id != 476271238273171458) return Task.CompletedTask;

			msg.Channel.SendMessageAsync($"<@{msg.Author.Id}>");
			return Task.CompletedTask;
        }

		private DiscordSocketClient _client;

		public async Task MainAsync()
		{
			string token = "";

			_client = new DiscordSocketClient();
			_client.Log += Log;
			_client.UserIsTyping += UserTyping;
			_client.MessageReceived += OnMessage;

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			await Task.Delay(-1);
		}
	}
}
