namespace be_rallyhubv2.contract.abstractions.results;

public sealed class Result<T>: Result
{
    private readonly T? _value;
    
    private Result(T? value, bool isSuccess, Error[] errors) : base(isSuccess, errors)
    {
        _value = value;
    }
    
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failed result cannot be accessed. ");

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(value, true, Array.Empty<Error>());
    }
    
    public new static Result<T> Failure(params Error[] errors) => new(default, false, errors);
    
    //return Result<BookingResponse>.Failure(BookingErrors.SlotTaken);   // thất bại
    //return Result<BookingResponse>.Success(response); 
}