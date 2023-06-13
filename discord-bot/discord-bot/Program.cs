using System;
using System.Threading.Tasks;

namespace bot
{
	public class Program
	{
		public static Task Main(string[] args) => MainAsync();

		public static async Task MainAsync()
		{
			var settings = BotSettings.Load("botsettings.txt");
			if (!settings.TryGetString("token", out var token))
            {
                Console.WriteLine("'token' is missing in botsettings.txt, terminating the bot!");
				return;
            }
			if (!settings.TryGetString("botname", out var botname))
            {
				botname = "3011";
            }

			var bot = new Bot(botname, token);

			bot.AddCommand(new HelpCommand(bot));
			bot.AddModule(UtilitiesModule.MakeModule("utils"));

			var replyModule = ReplyModule.MakeModule("reply");
			replyModule.LoadReplies();
			bot.AddModule(replyModule);

			var moviesModule = MoviesModule.MakeModule("movies");
            bot.AddModule(moviesModule);

			bot.Start();

			await Task.Delay(-1);
		}

		public static void PrepareEnvironment()
        {

        }
	}
}
