namespace FitnessStudioApi.Services;

public class BusinessRuleException : Exception
{
    public int StatusCode { get; }

    public BusinessRuleException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
