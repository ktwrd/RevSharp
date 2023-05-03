using kate.shared.Helpers;
using RevSharp.Core.Helpers;

namespace RevSharp.Core;

public partial class Client
{
    public event MessageDelegate MessageReceived;
    public event VoidDelegate Ready;
    public event VoidDelegate ClientAuthenticated;
    public event StringDelegate ErrorReceived;
}