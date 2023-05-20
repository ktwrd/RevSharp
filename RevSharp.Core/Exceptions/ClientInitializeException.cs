namespace RevSharp.Core;

public class ClientInitializeException : Exception
{
    public ClientInitializeException() : this("", "")
    {}

    public ClientInitializeException(string? message, string responseContent = "") : base(message)
    {
        ResponseContent = responseContent;
        Message = message ?? "";
    }
    public string ResponseContent { get; protected set; }
    public new string Message { get; protected set; }
}