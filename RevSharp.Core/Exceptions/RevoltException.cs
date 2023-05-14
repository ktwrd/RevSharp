namespace RevSharp.Core;

public class RevoltException : Exception
{
    public RevoltException() : this("", "")
    {}

    public RevoltException(string? message, string responseContent = "") : base(message)
    {
        ResponseContent = responseContent;
        Message = message ?? "";
    }
    public string ResponseContent { get; protected set; }
    public new string Message { get; protected set; }
}