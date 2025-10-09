using App.Infrastructure.Auth;
using FluentAssertions;

namespace App.Tests.Infrastructure.Auth;

public class BcryptPasswordHasherTests
{
    [Fact]
    public void Hash_And_Verify_Should_Succeed_For_Correct_Password()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher();
        var password = "My$trongP@ssword1!";

        // Act
        var hash = hasher.Hash(password);
        var ok = hasher.Verify(password, hash);
        var wrong = hasher.Verify("nope", hash);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2");
        hash.Length.Should().Be(60);
        ok.Should().BeTrue();
        wrong.Should().BeFalse();
    }

    [Fact]
    public void Hash_Should_Produce_Different_Hashes_For_Same_Password()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher();
        var password = "My$trongP@ssword1!";

        // Act
        var h1 = hasher.Hash(password);
        var h2 = hasher.Hash(password);

        // Assert
        h1.Should().NotBe(h2);
        hasher.Verify(password, h1).Should().BeTrue();
        hasher.Verify(password, h2).Should().BeTrue();
    }

    [Fact]
    public void Verify_Should_Return_False_For_Invalid_Hash_Format()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher();

        // Act
        var result = hasher.Verify("anything", "not-a-bcrypt-hash");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_Should_Fail_For_Case_Changed_Password()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher();
        var hash = hasher.Hash("Password123!");

        // Act
        var ok = hasher.Verify("password123!", hash);

        // Assert
        ok.Should().BeFalse();
    }

    [Fact]
    public void Verify_Should_Not_Trim_User_Password_But_May_Trim_Hash()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher();
        var hash = hasher.Hash("CleanPass");

        // Act
        var okWithSpaces = hasher.Verify("  CleanPass  ", hash); // user typo/spaces

        // Assert (policy: passwords are exact; spaces change it)
        okWithSpaces.Should().BeFalse();
    }

    [Fact]
    public void Verify_Should_Work_With_2y_Prefix_From_Other_Ecosystems()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher();
        var pwd = "abc123";

        // Create a valid hash, then simulate PHP/Ruby style by swapping to $2y$
        var aHash = hasher.Hash(pwd);               // e.g., $2a$11$...
        var yHash = string.Concat("$2y$", aHash.AsSpan(4));    // same salt+digest, different prefix

        // Act
        var ok = hasher.Verify(pwd, yHash);

        // Assert
        ok.Should().BeTrue();
    }
}
