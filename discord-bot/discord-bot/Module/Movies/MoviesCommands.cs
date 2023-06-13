using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal abstract class MoviesCommand : ICommand
    {
        public MoviesCommand(string keyword, string helpText, MoviesModule moviesModule)
        {
            Keyword = keyword;
            HelpText = helpText;
            _moviesModule = moviesModule;
        }

        public string Keyword { get; init; }

        public string HelpText { get; init; }

        public abstract void Execute(MessageWrapper msg);

        protected MoviesModule _moviesModule;
    }

    internal class AddMovieCommand : MoviesCommand
    {
        private static string helpText = "WIP\n";

        public AddMovieCommand(MoviesModule moviesModule)
            : base("add", helpText, moviesModule)
        { }

        public override void Execute(MessageWrapper msg)
        {
            msg.RawMsg.Channel.SendMessageAsync("It works, I guess.");
        }
    }
}
