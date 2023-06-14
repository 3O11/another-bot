using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace bot
{
    internal record class MovieRecord
    (
        Uri TmdbLink
    );

    internal class MovieStorage
    {
        public MovieStorage()
        {
            _movieRecords = new();
        }

        public bool TryAddMovie(ulong guildId, MovieRecord movie)
        {
            if (_movieRecords.TryGetValue(guildId, out var moviesTuple))
            {
                lock(moviesTuple.Item2)
                {
                    moviesTuple.Item1.Add(movie);
                    return true;
                }
            }

            return _movieRecords.TryAdd(guildId, new(new List<MovieRecord>{ movie }, new()));
        }

        public int GetMovieCount(ulong guildId)
        {
            if (_movieRecords.TryGetValue(guildId, out var moviesTuple))
            {
                lock(moviesTuple.Item2)
                {
                    return moviesTuple.Item1.Count;
                }
            }

            return 0;
        }

        public bool TryGetMovies(ulong guildId, out List<MovieRecord>? movies)
        {
            if (_movieRecords.TryGetValue(guildId, out var moviesTuple))
            {
                lock (moviesTuple.Item2)
                {
                    movies = new(moviesTuple.Item1);
                    return true;
                }
            }

            movies = null;
            return false;
        }

        public bool TryGetMovie(ulong guildId, int movieIndex, out MovieRecord? movie)
        {
            if (_movieRecords.TryGetValue(guildId, out var moviesTuple))
            {
                lock (moviesTuple.Item2)
                {
                    if (movieIndex < moviesTuple.Item1.Count)
                    {
                        movie = moviesTuple.Item1[movieIndex];
                        return true;
                    }
                }
            }

            movie = null;
            return false;
        }

        public bool TryRemoveMovie(ulong guildId, int movieIndex)
        {
            if (_movieRecords.TryGetValue(guildId, out var moviesTuple))
            {
                lock (moviesTuple.Item2)
                {
                    if (movieIndex < moviesTuple.Item1.Count)
                    {
                        moviesTuple.Item1.RemoveAt(movieIndex);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Save(ulong guildId = 0)
        {
            foreach (var id in guildId == 0 ? _movieRecords.Keys : new List<ulong> { guildId })
            {
                if (!TryGetMovies(guildId, out var movies))
                {
                    continue;
                }

                Directory.CreateDirectory("MovieStorage");
                using var file = new StreamWriter("MovieStorage/" + guildId.ToString() + ".json");
                file.Write(JsonSerializer.Serialize(movies));
            }

            return true;
        }

        public static MovieStorage Load()
        {
            var movieStorage = new MovieStorage();

            try
            {
                foreach (var filename in Directory.GetFiles("MovieStorage"))
                {
                    if (ulong.TryParse(Path.GetFileNameWithoutExtension(filename), out ulong guildId))
                    {
                        using var file = new StreamReader(filename);
                        var movies = JsonSerializer.Deserialize<List<MovieRecord>>(file.ReadToEnd());
                        if (movies != null)
                        {
                            movieStorage.SetMovies(guildId, movies);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("No movies saved yet.");
            }

            return movieStorage;
        }

        private void SetMovies(ulong guildId, List<MovieRecord> movies)
        {
            if (_movieRecords.TryGetValue(guildId, out var moviesTuple))
            {
                lock (moviesTuple.Item2)
                {
                    moviesTuple.Item1 = movies;
                }
            }
            else
            {
                _movieRecords.TryAdd(guildId, new(movies, new()));
            }
        }

        private ConcurrentDictionary<ulong, ValueTuple<List<MovieRecord>, object>> _movieRecords;
    }
}
