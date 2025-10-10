namespace server.Exceptions;

public class QuotaExceededException : Exception
{
    public QuotaExceededException(string message) : base(message) { }
}