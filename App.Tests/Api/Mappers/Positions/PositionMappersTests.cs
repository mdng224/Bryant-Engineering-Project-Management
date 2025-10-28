using App.Api.Contracts.Positions;
using App.Api.Mappers.Positions;
using App.Application.Common.Dtos;
using App.Application.Positions.Queries;
using App.Application.Positions.Queries.AddPosition;
using App.Application.Positions.Queries.GetPositions;
using FluentAssertions;

namespace App.Tests.Api.Mappers.Positions;

public class PositionMappersTests
{
[Fact]
    public void ToResponse_Maps_GetPositionsResult_To_GetPositionsResponse()
    {
        // arrange
        var dto = new PositionDto(
            Id: Guid.NewGuid(),
            Name: "Engineer",
            Code: "ENG",
            RequiresLicense: false
        );

        var result = new GetPositionsResult(
            Positions: [dto],
            TotalCount: 1,
            Page: 1,
            PageSize: 25,
            TotalPages: 1
        );

        // act
        var response = result.ToResponse();

        // assert
        response.Positions.Should().HaveCount(1);
        response.Positions[0].Id.Should().Be(dto.Id);
        response.Positions[0].Name.Should().Be(dto.Name);
        response.Positions[0].Code.Should().Be(dto.Code);
        response.Positions[0].RequiresLicense.Should().BeFalse();

        response.TotalCount.Should().Be(1);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(25);
        response.TotalPages.Should().Be(1);
    }

    [Fact]
    public void ToCommand_Maps_AddPositionRequest_To_AddPositionCommand()
    {
        // arrange
        var request = new AddPositionRequest(Name: "Planner", Code: "PLN", RequiresLicense: true);

        // act
        var command = request.ToCommand();

        // assert
        command.Name.Should().Be("Planner");
        command.Code.Should().Be("PLN");
        command.RequiresLicense.Should().BeTrue();
    }

    [Fact]
    public void ToResponse_Maps_AddPositionResult_To_AddPositionResponse()
    {
        // arrange
        var result = new AddPositionResult(
            Id: Guid.NewGuid(),
            Name: "Technician",
            Code: "TECH",
            RequiresLicense: false
        );

        // act
        var response = result.ToResponse();

        // assert
        response.Id.Should().Be(result.Id);
        response.Name.Should().Be(result.Name);
        response.Code.Should().Be(result.Code);
        response.RequiresLicense.Should().BeFalse();
    }

    [Fact]
    public void ToQuery_Maps_GetPositionsRequest_To_GetPositionsQuery()
    {
        // arrange
        var request = new GetPositionsRequest(Page: 2, PageSize: 50);

        // act
        var query = request.ToQuery();

        // assert
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(50);
    }
}