using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace bot
{
    internal class HelpModule : ModuleBase
    {
        HelpModule(string keyword)
        {
            Keyword = keyword;
        }

        public static HelpModule MakeModule(string keyword)
        {
            HelpModule module = new(keyword);



            return module;
        }
    }
}
