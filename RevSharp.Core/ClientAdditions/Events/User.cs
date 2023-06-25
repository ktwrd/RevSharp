using RevSharp.Core.Helpers;
using RevSharp.Core.Models.WebSocket;

namespace RevSharp.Core;

public partial class Client
{
    /// <summary>
    /// Emitted when a user is updated. Parameter will be the updated content.
    /// </summary>
    public event UserDelegate UserUpdate;

    /// <summary>
    /// - When exists in cache
    ///     - Invoke <see cref="Models.User.OnUpdate(UserUpdateMessage)"/>
    ///     - Invoke <see cref="UserUpdate"/>
    /// </summary>
    /// <param name="message"></param>
    internal void OnUserUpdate(UserUpdateMessage message)
    {
        if (UserCache.TryGetValue(message.UserId, out Models.User value))
        {
            value.OnUpdate(message);
            UserUpdate?.Invoke(value);
        }
    }

    public event UserDelegate UserRelationshipUpdate;

    /// <summary>
    /// - When exists in cache
    ///     - Invoke <see cref="Models.User.OnRelationshipUpdate(UserRelationshipEvent)"/>
    ///     - Invoke <see cref="UserRelationshipUpdate"/>
    /// </summary>
    /// <param name="message"></param>
    internal void OnUserRelationship(UserRelationshipEvent message)
    {
        if (UserCache.TryGetValue(message.UserId, out Models.User value))
        {
            value.OnRelationshipUpdate(message);
            UserRelationshipUpdate?.Invoke(value);
        }
    }
}