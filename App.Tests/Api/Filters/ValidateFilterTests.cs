using App.Api.Contracts.Auth;
using App.Api.Contracts.Auth.Requests;
using App.Api.Filters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace App.Tests.Api.Filters;

public class ValidateFilterTests
{
    // --- Tests ---------------------------------------------------------------

    [Fact]
    public async Task Valid_Payload_Invokes_Next_And_Bubbles_Result()
    {
        // Arrange
        var dto = new LoginRequest("user@example.com", "StrongPass1");
        var validator = new Mock<IValidator<LoginRequest>>();
        validator
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // valid

        var filter = new Validate<LoginRequest>(validator.Object);
        var ctx = MakeContext(dto);

        var expected = TypedResults.Ok(new { ok = true });
        var nextCalled = new[] { false };

        // Act
        var result = await filter.InvokeAsync(ctx, NextReturning(expected, nextCalled));

        // Assert
        Assert.True(nextCalled[0]);
        Assert.Same(expected, result);
        validator.Verify(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Null_Body_Returns_BadRequest_And_DoesNot_Invoke_Next()
    {
        // Arrange
        var validator = new Mock<IValidator<LoginRequest>>();
        var filter = new Validate<LoginRequest>(validator.Object);
        var ctx = MakeContext(arg: null);

        var nextCalled = new[] { false };

        // Act
        var result = await filter.InvokeAsync(ctx, NextReturning(TypedResults.Ok(new { ok = true }), nextCalled));

        // Assert
        var bad = Assert.IsType<BadRequest<string>>(result);
        Assert.Equal("Invalid request payload.", bad.Value);
        Assert.False(nextCalled[0]);
        validator.Verify(v => v.ValidateAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Invalid_Payload_Returns_ValidationProblem_And_DoesNot_Invoke_Next()
    {
        // Arrange
        var dto = new LoginRequest("", "short");
        var failures = new List<ValidationFailure>
        {
            new(nameof(LoginRequest.Email), "Email is required."),
            new(nameof(LoginRequest.Email), "A valid email address is required."),
            new(nameof(LoginRequest.Password), "Password must be at least 8 characters long.")
        };

        var validator = new Mock<IValidator<LoginRequest>>();
        validator
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var filter = new Validate<LoginRequest>(validator.Object);
        var ctx = MakeContext(dto);

        var nextCalled = new[] { false };

        // Act
        var result = await filter.InvokeAsync(ctx, NextReturning(TypedResults.Ok(new { ok = true }), nextCalled));

        // Assert
        var vp = Assert.IsType<ValidationProblem>(result);

        Assert.True(vp.ProblemDetails.Errors.TryGetValue(nameof(LoginRequest.Email), out var emailErrors));
        Assert.Contains("Email is required.", emailErrors);
        Assert.Contains("A valid email address is required.", emailErrors);

        Assert.True(vp.ProblemDetails.Errors.TryGetValue(nameof(LoginRequest.Password), out var pwdErrors));
        Assert.Contains("Password must be at least 8 characters long.", pwdErrors);

        Assert.False(nextCalled[0]);
        validator.Verify(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Passes_RequestAborted_CancellationToken_To_Validator()
    {
        // Arrange
        var dto = new LoginRequest("user@example.com", "StrongPass1");
        var validator = new Mock<IValidator<LoginRequest>>();

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        validator
            .Setup(v => v.ValidateAsync(dto, It.Is<CancellationToken>(t => t == token)))
            .ReturnsAsync(new ValidationResult());

        var filter = new Validate<LoginRequest>(validator.Object);
        var ctx = MakeContext(dto, aborted: token);

        // Act
        var result = await filter.InvokeAsync(ctx, NextReturning(TypedResults.Ok()));

        // Assert
        Assert.IsType<Ok>(result);
        validator.Verify(v => v.ValidateAsync(dto, It.Is<CancellationToken>(t => t == token)), Times.Once);
    }

    // --- Helpers -------------------------------------------------------------
    private sealed class TestInvocationContext : EndpointFilterInvocationContext
    {
        public override HttpContext HttpContext { get; }
        public override object?[] Arguments { get; }

        public TestInvocationContext(HttpContext httpContext, params object?[] args)
        {
            HttpContext = httpContext;
            Arguments = args;
        }

        public override T GetArgument<T>(int index) => (T)Arguments[index]!;
    }

    private static TestInvocationContext MakeContext(object? arg, CancellationToken? aborted = null)
    {
        var http = new DefaultHttpContext();
        if (aborted is { } ct) http.RequestAborted = ct;
        return new TestInvocationContext(http, arg);
    }

    private static EndpointFilterDelegate NextReturning(object? value, bool[]? calledFlag = null)
        => ctx =>
        {
            if (calledFlag is not null && calledFlag.Length > 0) calledFlag[0] = true;
            return ValueTask.FromResult(value);
        };
}
