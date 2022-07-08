using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot
{
    internal interface ICommand
    {
        public string Keyword { get; }
        public string HelpText { get; }
        public bool Execute(MessageWrapper msg);
    }
}
