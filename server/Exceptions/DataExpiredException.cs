namespace server.Exceptions;

public class DataExpiredException : Exception
{
    public DataExpiredException(string message) : base(message)
    {
    }
}