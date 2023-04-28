using RevSharp.Core.Models;

namespace RevSharp.Core.Helpers;

public delegate void GenericDelegate<T>(T content);
public delegate void MessageDelegate(Message message);