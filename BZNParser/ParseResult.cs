using System.Runtime.ExceptionServices;

public class ParseResult
{
    public bool Success { get; }
    public float Probability { get; }
    public string? Error { get; }

    private ParseResult(bool success, string? error = null)
    {
        Success = success;
        Probability = success ? 1f : 0f;
        Error = error;
    }
    private ParseResult(float likely, string? error = null)
    {
        Success = likely > 0;
        Probability = likely;
        Error = error;
    }
    private ParseResult(string errorInfo)
    {
        Success = false;
        Probability = 0f;
        Error = errorInfo;
    }

    public static ParseResult Ok() => new ParseResult(true);
    public static ParseResult Ok(float likely) => new ParseResult(likely);

    public static ParseResult Fail(string error) => new ParseResult(error);
}