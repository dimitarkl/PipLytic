namespace PipLytic.Api.Exceptions;

public class DataExpiredException : Exception
{
    public DataExpiredException(string message) : base(message)
    {
    }
}