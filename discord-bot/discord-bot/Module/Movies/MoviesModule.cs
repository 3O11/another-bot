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

        public static MoviesModule MakeModule(string keyword = "movies")
        {
            var module = new MoviesModule(keyword);

            module._moduleDescription =
                "[WIP] A module to help with organizing movie nights. " +
                "Supports adding movies to a movie \"lottery\" and actually " +
                "picking a movie to watch.";



            module.AddCommand(new AddMovieCommand(module));

            return module;
        }
    }
}
