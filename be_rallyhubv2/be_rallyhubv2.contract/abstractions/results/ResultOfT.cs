using System.Text.Json.Serialization;

namespace be_rallyhubv2.contract.abstractions.results;

public sealed class Result<T>: Result
{
    private readonly T? _value;
    
    [JsonConstructor]
    //kêu thằng jsonConstructor này dựng cái constructor private này thành object
    //mình chỉ cache những kết quả thành công thôi, nên thường list errors sẽ empty
    private Result(T? value, bool isSuccess, IReadOnlyList<Error> errors) : base(isSuccess, errors)
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