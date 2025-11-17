using App.Application.Abstractions.Persistence.Readers;
using App.Application.Clients.Queries.GetClientLookups;
using App.Domain.Clients;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Clients.Queries;

public sealed class GetClientLookupsHandlerTests
{
     private readonly Mock<IClientCategoryReader> _categoryReader = new();
    private readonly Mock<IClientTypeReader> _typeReader = new();
    private readonly GetClientLookupsHandler _handler;

    public GetClientLookupsHandlerTests()
    {
        _handler = new GetClientLookupsHandler(_categoryReader.Object, _typeReader.Object);
    }

    [Fact]
    public async Task Returns_Lookups_From_Readers()
    {
        // Arrange
        var ct = CancellationToken.None;

        var categoryId1 = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();

        var categories = new[]
        {
            new ClientCategory(categoryId1, "Municipality"),
            new ClientCategory(categoryId2, "Developer")
        };

        var types = new[]
        {
            new ClientType(Guid.NewGuid(), "City Government", "City-level government",   categoryId1),
            new ClientType(Guid.NewGuid(), "County Government", "County-level govt",     categoryId1),
            new ClientType(Guid.NewGuid(), "Private Developer", "Private development",   categoryId2),
        };

        _categoryReader
            .Setup(r => r.GetAllAsync(ct))
            .ReturnsAsync(categories);

        _typeReader
            .Setup(r => r.GetAllAsync(ct))
            .ReturnsAsync(types);

        var query = new GetClientLookupsQuery();

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;

        dto.Categories.Should().HaveCount(2);
        dto.Types.Should().HaveCount(3);

        // verify categories mapped correctly
        dto.Categories.Select(c => c.Id).Should().BeEquivalentTo(new[]
        {
            categoryId1,
            categoryId2
        });

        dto.Categories.Select(c => c.Name).Should().BeEquivalentTo(new[]
        {
            "Municipality",
            "Developer"
        });

        // verify types mapped correctly
        dto.Types.Should().ContainSingle(t => t.Name == "City Government"    && t.CategoryId == categoryId1);
        dto.Types.Should().ContainSingle(t => t.Name == "County Government"  && t.CategoryId == categoryId1);
        dto.Types.Should().ContainSingle(t => t.Name == "Private Developer"  && t.CategoryId == categoryId2);

        // verify readers called once
        _categoryReader.Verify(r => r.GetAllAsync(ct), Times.Once);
        _typeReader.Verify(r => r.GetAllAsync(ct), Times.Once);
    }

    [Fact]
    public async Task Returns_Empty_Collections_When_No_Lookups()
    {
        // Arrange
        var ct = CancellationToken.None;

        _categoryReader
            .Setup(r => r.GetAllAsync(ct))
            .ReturnsAsync([]);

        _typeReader
            .Setup(r => r.GetAllAsync(ct))
            .ReturnsAsync([]);

        var query = new GetClientLookupsQuery();

        // Act
        var result = await _handler.Handle(query, ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;

        dto.Categories.Should().BeEmpty();
        dto.Types.Should().BeEmpty();

        _categoryReader.Verify(r => r.GetAllAsync(ct), Times.Once);
        _typeReader.Verify(r => r.GetAllAsync(ct), Times.Once);
    }
}