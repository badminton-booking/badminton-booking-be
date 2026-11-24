using be_rallyhubv2.contract.abstractions.results;

namespace be_rallyhubv2.tests.unit.abstractions;

public class ResultTests
{
    [Fact]
    public void Success_has_no_errors()
    {
        var result = Result.Success();
        Assert.Empty(result.Errors);
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public void Failure_without_error_throws()
        => Assert.Throws<InvalidOperationException>(() => Result.Failure());

    [Fact]
    public void Failure_keeps_the_error()
    {
        var result = Result<int>.Failure(Error.NotFound("Common.NotFound"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Errors[0].Type);
    }

    [Fact]
    public void Reading_value_of_failed_result_throws()
        => Assert.Throws<InvalidOperationException>(() => Result<int>.Failure(Error.Failure("x")).Value);

    [Fact]
    public void Success_returns_value()
        => Assert.Equal(5, Result<int>.Success(5).Value);

    [Fact]
    public void Success_with_null_throws()
        => Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
}