using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal class MoviesModule : ModuleBase
    {
        MoviesModule(string keyword = "movies")
        {
            Keyword = keyword;
        }

        public static MoviesModule MakeModule(string keyword = "movies", string tmdbToken = "none")
        {
            var module = new MoviesModule(keyword);

            module._moduleDescription =
                "[WIP] A module to help with organizing movie nights. " +
                "Supports adding movies to a movie \"lottery\" and actually " +
                "picking a movie to watch.";

            var movieStorage = MovieStorage.Load();

            module.AddCommand(new AddMovieCommand(movieStorage));
            module.AddCommand(new ListMoviesCommand(movieStorage));
            module.AddCommand(new RemoveMovieCommand(movieStorage));
            module.AddCommand(new InfoMovieCommand(movieStorage, tmdbToken));
            module.AddCommand(new LotteryMovieCommand(movieStorage, tmdbToken));

            return module;
        }
    }
}
