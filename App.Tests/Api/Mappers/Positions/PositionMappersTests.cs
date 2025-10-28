using App.Api.Contracts.Positions;
using App.Api.Mappers.Positions;
using App.Application.Common.Dtos;
using App.Application.Positions.Queries;
using FluentAssertions;

namespace App.Tests.Api.Mappers.Positions;

public class PositionMappersTests
{
    [Fact]
    public void ToResponse_Maps_All_Fields()
    {
        // arrange
        var dto = new PositionDto(Id: Guid.NewGuid(),
            Name: "Engineer",
            Code: "ENG",
            RequiresLicense: false);
        var result = new GetPositionsResult(
            [dto],
            TotalCount: 1,
            Page: 1,
            PageSize: 25,
            TotalPages: 1);

        // act
        var response = result.ToResponse();

        // assert
        response.Positions.Should().HaveCount(1);
        response.Positions[0].Name.Should().Be("Engineer");
        response.TotalCount.Should().Be(1);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(25);
        response.TotalPages.Should().Be(1);
    }
}