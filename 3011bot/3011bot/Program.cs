using System.Threading.Tasks;

namespace bot
{
	public class Program
	{
		public static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync()
		{
			var bot = new Bot("3011", "Insert your token here.");

			bot.AddCommand(new HelpCommand(bot));
			bot.AddModule(ReplyModule.MakeModule("reply"));
			bot.AddModule(UtilitiesModule.MakeModule("utils"));

			bot.Start();

			await Task.Delay(-1);
		}
	}
}
