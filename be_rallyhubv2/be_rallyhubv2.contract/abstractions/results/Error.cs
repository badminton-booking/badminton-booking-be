namespace be_rallyhubv2.contract.abstractions.results;

//định nghĩa một lỗi gồm mã, lỗi và tham số
public sealed record class Error(string Code, ErrorType Type, params object[] Args)
{
    public static Error Failure(string code, params object[] args) => new(code, ErrorType.Failure, args);
    public static Error Validation(string code, params object[] args) => new(code, ErrorType.Validation, args);
    public static Error NotFound(string code, params object[] args) => new(code, ErrorType.NotFound, args);
    public static Error Conflict(string code, params object[] args) => new(code, ErrorType.Conflict, args);
    public static Error Unauthorized(string code, params object[] args) => new(code, ErrorType.Unauthorized, args);
    public static Error Forbidden(string code, params object[] args) => new(code, ErrorType.Forbidden, args);
}