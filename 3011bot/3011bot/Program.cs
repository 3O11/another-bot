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
			var bot = new Bot("3022", "Insert your token here.");

			// Add modules
			bot.AddModule(new RepliesModule());
			bot.AddModule(new UtilitiesModule());

			bot.Start();

			await Task.Delay(-1);
		}
	}
}
