using App.Infrastructure.Auth;
using FluentAssertions;

namespace App.Tests.Infrastructure.Auth;

public class TokenGeneratorTests
{
    [Fact]
    public void CreateTokenPair_Should_Return_UrlSafe_Token_And_Valid_Hash()
    {
        // Act
        var (rawToken, tokenHash) = TokenGenerator.CreateTokenPair();

        // Assert
        rawToken.Should().NotBeNullOrWhiteSpace("a token should always be generated");
        tokenHash.Should().NotBeNullOrWhiteSpace("the hash should always be generated");
        rawToken.Should().NotContainAny("+", "/", "=", "because the token must be URL-safe");
        rawToken.Length.Should().Be(43, "a 32-byte Base64 string (trimmed of '=') should be 43 chars long");
        tokenHash.Length.Should().Be(64, "a SHA256 hash in hex should be 64 characters long");
        tokenHash.Should().MatchRegex("^[0-9A-F]+$", "Convert.ToHexString outputs uppercase hex characters");
    }

    [Fact]
    public void CreateTokenPair_Should_Hash_Match_HashMethod()
    {
        // Arrange
        var (rawToken, expectedHash) = TokenGenerator.CreateTokenPair();

        // Act
        var actualHash = TokenGenerator.Hash(rawToken);

        // Assert
        actualHash.Should().Be(expectedHash, "Hash(rawToken) should match the tokenHash generated from CreateTokenPair()");
    }

    [Fact]
    public void Hash_Should_Produce_Known_SHA256_For_ABC()
    {
        // Arrange
        const string input = "abc";
        const string expected =
            "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD";

        // Act
        var actual = TokenGenerator.Hash(input);

        // Assert
        actual.Should().Be(expected, "the SHA256 of 'abc' should match the known hex value");
    }

    [Fact]
    public void Hash_Should_Throw_When_Input_Is_Null()
    {
        // Arrange
        string? nullInput = null;

        // Act
        var act = () => TokenGenerator.Hash(nullInput!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*Value cannot be null*");
    }

    [Fact]
    public void CreateTokenPair_Should_Generate_Unique_Tokens()
    {
        // Arrange
        const int count = 100;

        // Act
        var tokens = Enumerable.Range(0, count)
            .Select(_ => TokenGenerator.CreateTokenPair().rawToken)
            .ToArray();

        // Assert
        tokens.Should().OnlyHaveUniqueItems("tokens are randomly generated and should not collide");
    }
}