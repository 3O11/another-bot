using System.Threading.Tasks;

namespace bot
{
	public class Program
	{
		public static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync()
		{
			var bot = new Bot("3011", "NTQwNjYxOTIxNTQ0OTI5Mjk4.XFN4pw.1yTaeEwl2GIuCHSl2aHgDo5g3Rg");
			//var bot = new Bot("3011", "Insert your token here.");

			bot.AddCommand(new HelpCommand(bot));
			bot.AddModule(ReplyModule.MakeModule("reply"));
			bot.AddModule(UtilitiesModule.MakeModule("utils"));

			bot.Start();

			await Task.Delay(-1);
		}
	}
}
