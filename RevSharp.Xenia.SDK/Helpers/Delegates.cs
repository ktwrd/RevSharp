using RevSharp.Core.Models;
using RevSharp.Xenia.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevSharp.Xenia.Helpers
{
    public delegate void CommandExecuteDelegate(
        Server? server,
        User? author,
        BaseChannel? channel,
        INamedChannel? namedChannel,
        CommandInfo info,
        BaseModule module);
}
