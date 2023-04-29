using System.Runtime.Serialization;

namespace RevSharp.Core.Models;

public enum UserPresence
{
    /// <summary>
    /// User is online
    /// </summary>
    [EnumMember(Value = "Online")]
    Online,
    /// <summary>
    /// User is not currently available
    /// </summary>
    [EnumMember(Value = "Idle")]
    Idle,
    /// <summary>
    /// User is focusing / will only receive mentions
    /// </summary>
    [EnumMember(Value = "Focus")]
    Focus,
    /// <summary>
    /// User is busy / will not receive any notifications
    /// </summary>
    [EnumMember(Value = "Busy")]
    Busy,
    /// <summary>
    /// User appears to be offline
    /// </summary>
    [EnumMember(Value = "Invisible")]
    Invisible
}