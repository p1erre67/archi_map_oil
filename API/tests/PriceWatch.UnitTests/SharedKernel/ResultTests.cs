using FluentAssertions;
using PriceWatch.SharedKernel.Domain.Results;

namespace PriceWatch.UnitTests.SharedKernel;

public class ResultTests
{
    [Fact]
    public void Success_ShouldBeSuccess()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Failure_ShouldBeFailure()
    {
        var error = Error.Validation("Test", "Something failed");

        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_ShouldNotHaveError()
    {
        var result = Result.Success();

        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void SuccessWithValue_ShouldReturnValue()
    {
        var result = Result.Success(42);

        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_AccessingValue_ShouldThrow()
    {
        var result = Result.Failure<int>(Error.NotFound("Test", "Not found"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_ValueToResult()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_ErrorToResult()
    {
        var error = Error.Validation("Field", "Invalid");

        Result result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
