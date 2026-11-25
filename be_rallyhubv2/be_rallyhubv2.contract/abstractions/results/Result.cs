namespace be_rallyhubv2.contract.abstractions.results;

public class Result
{
    //chỉ có lớp con mới được gọi constructor đó, code bên ngoài thì không
    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
        {
            throw new InvalidOperationException("A successful result cannot contain errors.");
        }

        if (!isSuccess && errors.Count == 0)
        {
            throw new InvalidOperationException("A failed result must contain at least one error.");
        }
        
        IsSuccess = isSuccess;
        Errors = errors;
    }
    public bool IsSuccess  { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }
    
    public static Result Success() => new(true, Array.Empty<Error>()); 
    public static Result Failure(params Error[] errors) => new (false, errors);

}