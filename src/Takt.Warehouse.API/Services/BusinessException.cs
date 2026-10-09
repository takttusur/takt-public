namespace Takt.Warehouse.API.Services;

public sealed class BusinessException(string code, string title, int statusCode, string detail) : Exception(detail)
{
    public string Code { get; } = code;
    public string Title { get; } = title;
    public int StatusCode { get; } = statusCode;
}
