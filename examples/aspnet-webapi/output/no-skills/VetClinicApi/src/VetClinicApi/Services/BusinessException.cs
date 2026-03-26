namespace VetClinicApi.Services;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
