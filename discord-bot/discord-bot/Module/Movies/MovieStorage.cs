using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.Module.Movies
{
    internal record class MovieRecord
    (
        string MovieTitle,
        Uri ImdbLink
    );

    internal class MovieStorage
    {
        public 

        public static bool Save()
        {
            return true;
        }

        public static MovieStorage Load()
        {

        }
    }
}
