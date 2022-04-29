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

		public async Task MainAsync()
		{
			var bot = new Bot("3022");

			// Add modules
			var replies = new RepliesModule();
			bot.RegisterModule(replies);
			var utils = new UtilitiesModule();
			bot.RegisterModule(utils);

			bot.Start();

			await Task.Delay(-1);
		}
	}
}
