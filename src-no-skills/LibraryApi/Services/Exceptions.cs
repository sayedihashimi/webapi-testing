namespace LibraryApi.Services;

public class BusinessRuleException : Exception
{
    public int StatusCode { get; }

    public BusinessRuleException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
