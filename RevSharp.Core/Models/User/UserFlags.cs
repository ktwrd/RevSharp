namespace RevSharp.Core.Models;

public enum UserFlags
{
    /// <summary>
    /// User has been suspended from the platform
    /// </summary>
    Suspended = 1,
    /// <summary>
    /// User has deleted their account
    /// </summary>
    Deleted = 2,
    /// <summary>
    /// User was banned off the platform
    /// </summary>
    Banned = 4
}