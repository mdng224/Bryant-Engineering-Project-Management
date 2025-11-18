using App.Application.Abstractions.Persistence.Readers;
using App.Application.Projects.Queries.GetProjectLookups;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Projects.Queries;

public sealed class GetProjectLookupsHandlerTests
{
    private readonly Mock<IProjectReader> _projectReader = new();

    private GetProjectLookupsHandler CreateSut() => new(_projectReader.Object);

    [Fact]
    public async Task Handle_WhenReaderReturnsManagers_ReturnsOkResultWithManagers()
    {
        // Arrange
        var managers = new List<string> { "Andy Smith", "Beth Jones" };
        _projectReader
            .Setup(r => r.GetDistinctProjectManagersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(managers);

        var sut = CreateSut();
        var query = new GetProjectLookupsQuery();
        var ct = CancellationToken.None;

        // Act
        var result = await sut.Handle(query, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Managers.Should().BeEquivalentTo(
            managers,
            options => options.WithStrictOrdering()
        );

        _projectReader.Verify(
            r => r.GetDistinctProjectManagersAsync(ct),
            Times.Once
        );
        _projectReader.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenReaderReturnsEmptyList_ReturnsOkResultWithEmptyManagers()
    {
        // Arrange
        _projectReader
            .Setup(r => r.GetDistinctProjectManagersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetProjectLookupsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Managers.Should().BeEmpty();

        _projectReader.Verify(
            r => r.GetDistinctProjectManagersAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _projectReader.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToReader()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _projectReader
            .Setup(r => r.GetDistinctProjectManagersAsync(token))
            .ReturnsAsync(new List<string> { "Andy" });

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(new GetProjectLookupsQuery(), token);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _projectReader.Verify(
            r => r.GetDistinctProjectManagersAsync(token),
            Times.Once
        );
        _projectReader.VerifyNoOtherCalls();
    }
}
