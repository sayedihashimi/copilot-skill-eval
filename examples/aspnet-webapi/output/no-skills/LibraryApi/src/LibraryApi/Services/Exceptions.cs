namespace LibraryApi.Services;

public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
