namespace csFloatTracker.Utils;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new Result<T>(true, value, null);

    public static async Task<Result<T>> TryAsync(Func<Task<T>> func)
    {
        try
        {
            return Ok(await func());
        }
        catch (Exception ex)
        {
            return Fail(ex.Message);
        }
    }

    public static Result<T> Fail(string error) => new Result<T>(false, default, error);
}

