using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;

namespace bot
{
    internal abstract class MoviesCommand : ICommand
    {
        public MoviesCommand(string keyword, string helpText, MovieStorage movieStorage)
        {
            Keyword = keyword;
            HelpText = helpText;
            _movieStorage = movieStorage;
        }

        public string Keyword { get; init; }

        public string HelpText { get; init; }

        public abstract void Execute(MessageWrapper msg);

        protected MovieStorage _movieStorage;
    }

    internal class AddMovieCommand : MoviesCommand
    {
        private static string helpText =
            "[WIP]\n" +
            "Usage: <botname> <movies> add <TMDB URL>\n" +
            "\n" +
            "Takes a link to the movie on the TMDB (https://www.themoviedb.org/)";

        public AddMovieCommand(MovieStorage movieStorage)
            : base("add", helpText, movieStorage)
        { }

        public override void Execute(MessageWrapper msg)
        {
            ulong guild = Utils.GetGuild(msg.RawMsg).Id;
            var movie = new MovieRecord(new Uri(msg.Content));

            if (!_movieStorage.TryAddMovie(guild, movie))
            {
                msg.RawMsg.Channel.SendMessageAsync("Failed to add movie.");
            }

            if (!_movieStorage.Save(guild))
            {
                msg.RawMsg.Channel.SendMessageAsync("Failed to save movies.");
            }

            msg.RawMsg.Channel.SendMessageAsync("Movie was added successfully.");
        }
    }

    internal class InfoMovieCommand : MoviesCommand
    {
        private static string helpText =
            "[WIP]\n" +
            "Usage: <botname> <movies> info <index>\n" +
            "\n" +
            "Gets info about the movie at the specified index from TMDB (https://www.themoviedb.org/)";

        private static Regex movieIdExtractor = new Regex(@"^https://www.themoviedb.org/movie/(?<movieId>[0-9]+)-[a-zA-Z0-9\-]+$", RegexOptions.Compiled);

        private string _tmdbkey;

        public InfoMovieCommand(MovieStorage movieStorage, string tmdbkey)
            : base("info", helpText, movieStorage)
        {
            _tmdbkey = tmdbkey;
        }

        public override void Execute(MessageWrapper msg)
        {
            if(!int.TryParse(msg.Content, out int index))
            {
                msg.RawMsg.Channel.SendMessageAsync("Invalid index.");
                return;
            }

            ulong guild = Utils.GetGuild(msg.RawMsg).Id;
            if (!_movieStorage.TryGetMovie(guild, index - 1, out var movie))
            {
                msg.RawMsg.Channel.SendMessageAsync("Could not find this movie.");
                return;
            }

            var match = movieIdExtractor.Match(movie.TmdbLink.OriginalString);
            string movieIdStr = match.Groups["movieId"].Value;
            var client = new RestClient("https://api.themoviedb.org/3/movie/" + movieIdStr + "?language=en-US");
            var request = new RestRequest();
            request.AddHeader("accept", "application/json");
            request.AddHeader("Authorization", "Bearer " + _tmdbkey);
            RestResponse response = client.Execute(request);
            msg.RawMsg.Channel.SendMessageAsync("```" + response.Content.Substring(0, Math.Min(response.Content.Length, 1900)) + "```");
        }
    }

    internal class LotteryMovieCommand : MoviesCommand
    {
        private static string helpText =
            "[WIP]\n" +
            "Usage: <botname> <movies> lottery\n" +
            "\n" +
            "Picks a random movie from the list and replies with the its info.";

        private static Regex movieIdExtractor = new Regex(@"^https://www.themoviedb.org/movie/(?<movieId>[0-9]+)-[a-zA-Z0-9\-]+$", RegexOptions.Compiled);

        private Random _rng = new();
        private string _tmdbkey;

        public LotteryMovieCommand(MovieStorage movieStorage, string tmdbkey)
            : base("lottery", helpText, movieStorage)
        {
            _tmdbkey = tmdbkey;
        }

        public override void Execute(MessageWrapper msg)
        {
            ulong guild = Utils.GetGuild(msg.RawMsg).Id;
            int movieCount = _movieStorage.GetMovieCount(guild);
            int selectedMovie = _rng.Next() % movieCount;

            if (!_movieStorage.TryGetMovie(guild, selectedMovie, out var movie))
            {
                msg.RawMsg.Channel.SendMessageAsync("Could not find this movie.");
                return;
            }

            var match = movieIdExtractor.Match(movie.TmdbLink.OriginalString);
            string movieIdStr = match.Groups["movieId"].Value;
            var client = new RestClient("https://api.themoviedb.org/3/movie/" + movieIdStr + "?language=en-US");
            var request = new RestRequest();
            request.AddHeader("accept", "application/json");
            request.AddHeader("Authorization", "Bearer " + _tmdbkey);
            RestResponse response = client.Execute(request);
            msg.RawMsg.Channel.SendMessageAsync($"Selected movie at index: {selectedMovie + 1}\n```" + response.Content.Substring(0, Math.Min(response.Content.Length, 1900)) + "```");
        }
    }

    internal class RemoveMovieCommand : MoviesCommand
    {
        private static string helpText =
            "Usage: <botname> <movies> remove <index>\n" +
            "\n" +
            "Removes the movie at the specified index.";

        public RemoveMovieCommand(MovieStorage movieStorage)
            : base("remove", helpText, movieStorage)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if(!int.TryParse(msg.Content, out int index))
            {
                msg.RawMsg.Channel.SendMessageAsync("Invalid index.");
                return;
            }

            ulong guild = Utils.GetGuild(msg.RawMsg).Id;
            if (!_movieStorage.TryRemoveMovie(guild, index - 1))
            {
                msg.RawMsg.Channel.SendMessageAsync("Failed to remove movie.");
                return;
            }

            if (!_movieStorage.Save(guild))
            {
                msg.RawMsg.Channel.SendMessageAsync("Failed to save movies.");
                return;
            }

            msg.RawMsg.Channel.SendMessageAsync("Removed movie successfully.");
        }
    }

    internal class ListMoviesCommand : MoviesCommand
    {
        private static string helpText =
            "Usage: <botname> <movies> list\n" +
            "\n" +
            "Lists currently saved movies. " +
            "Takes no parameters.";

        public ListMoviesCommand(MovieStorage movieStorage)
            : base ("list", helpText, movieStorage)
        { }

        public override void Execute(MessageWrapper msg)
        {
            if (msg.Content != "")
            {
                msg.RawMsg.Channel.SendMessageAsync("This command takes no parameters.");
                return;
            }

            ulong guild = Utils.GetGuild(msg.RawMsg).Id;
            if (!_movieStorage.TryGetMovies(guild, out var movies))
            {
                msg.RawMsg.Channel.SendMessageAsync("Failed to retrieve movie list.");
                return;
            }

            if (movies.Count == 0)
            {
                msg.RawMsg.Channel.SendMessageAsync("There are currently no movies.");
                return;
            }

            int length = (movies.Count + 1).ToString().Length;
            var reply = new StringBuilder();
            reply.AppendLine("```");
            for (int i = 0; i < movies.Count; ++i)
            {
                reply.AppendLine((i + 1).ToString().PadLeft(length) + ": " + movies[i].TmdbLink);
            }
            reply.AppendLine("```");

            msg.RawMsg.Channel.SendMessageAsync(reply.ToString());
        }
    }
}
