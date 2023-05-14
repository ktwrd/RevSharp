namespace RevSharp.Core.Models;

public enum UserPermission : long
{
    Access = 1L << 0,
    ViewProfile = 1L << 1,
    SendMessage = 1L << 2,
    Invite = 1L << 3
}