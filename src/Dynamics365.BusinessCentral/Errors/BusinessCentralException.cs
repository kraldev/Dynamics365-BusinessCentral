using System.Net;

namespace Dynamics365.BusinessCentral.Errors;

public sealed class BusinessCentralException : Exception
{
    public string Code { get; }
    public HttpStatusCode StatusCode { get; }

    public BusinessCentralException(string code, string message, HttpStatusCode statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
