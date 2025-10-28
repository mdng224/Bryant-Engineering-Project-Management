using App.Api.Contracts.Employees;
using App.Api.Features.Employees;
using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Employees.Queries;
using App.Application.Positions.Queries;
using App.Application.Positions.Queries.GetPositions;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
// IMPORTANT: allow R.Ok / R.Fail
using static App.Application.Common.R;

namespace App.Tests.Application.Employees.Queries;

public class GetEmployeesHandlerTests
{
    private static readonly Guid PosA = Guid.NewGuid();
    private static readonly Guid PosB = Guid.NewGuid();

    [Fact]
    public async Task Handle_Should_Return_Ok_When_Both_Queries_Succeed()
    {
        // Arrange
        var request = new GetEmployeesRequest(Page: 1, PageSize: 10, Name: "doe");

        var employeesDtos = new List<EmployeeDto>
        {
            new(
                Id: Guid.NewGuid(),
                UserId: null,
                FirstName: "John",
                LastName: "Doe",
                PreferredName: "Johnny",
                EmploymentType: "FullTime",
                SalaryType: "Salary",
                HireDate: DateTimeOffset.UtcNow.AddYears(-1),
                EndDate: null,
                Department: "Engineering",
                PositionIds: new List<Guid> { PosA, PosB },
                CompanyEmail: "john.doe@corp.com",
                WorkLocation: "HQ",
                LicenseNotes: null,
                Notes: null,
                RecommendedRoleId: null,
                IsPreapproved: false,
                CreatedAtUtc: DateTimeOffset.UtcNow.AddYears(-1),
                UpdatedAtUtc: DateTimeOffset.UtcNow,
                DeletedAtUtc: null,
                IsActive: true
            )
        };

        var totalCount = employeesDtos.Count;
        const int pageSize = 10;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var employeesResult = new GetEmployeesResult(
            EmployeesDtos: employeesDtos,
            TotalCount: totalCount,
            Page: 1,
            PageSize: pageSize,
            TotalPages: totalPages
        );

        var mockEmployeesHandler =
            new Mock<IQueryHandler<GetEmployeesQuery, Result<GetEmployeesResult>>>();

        mockEmployeesHandler
            .Setup(h => h.Handle(It.IsAny<GetEmployeesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Ok(employeesResult));

        var positionsResult = new GetPositionsResult(
            Positions: new List<PositionDto>
            {
                new(PosA, "Engineer", "ENG", false),
                new(PosB, "Manager", "MGR", false)
            },
            TotalCount: 2,
            Page: 1,
            PageSize: 200,   // whatever your repo caps at (e.g., MaxPageSize)
            TotalPages: 1
        );

        var mockPositionsHandler =
            new Mock<IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>>>();

        mockPositionsHandler
            .Setup(h => h.Handle(It.IsAny<GetPositionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Ok(positionsResult));

        // Act
        var result = await GetEmployees.Handle(
            request,
            mockEmployeesHandler.Object,
            mockPositionsHandler.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<Ok<GetEmployeesResponse>>();
        var ok = result.As<Ok<GetEmployeesResponse>>();
        ok.Value.Should().NotBeNull();

        ok.Value!.Employees.Should().HaveCount(1);
        ok.Value!.TotalCount.Should().Be(1);
        ok.Value!.Page.Should().Be(1);
        ok.Value!.PageSize.Should().Be(10);
        ok.Value!.TotalPages.Should().Be(totalPages);

        var employee = ok.Value!.Employees[0];

        // Order-insensitive and avoids C# 12 collection expressions
        employee.Details.PositionNames
            .Should()
            .BeEquivalentTo(new[] { "Engineer", "Manager" });

        mockPositionsHandler.Verify(
            h => h.Handle(It.IsAny<GetPositionsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Problem_When_Employees_Query_Fails()
    {
        // Arrange
        var request = new GetEmployeesRequest(Page: 1, PageSize: 10, Name: "doe");

        var mockEmployeesHandler =
            new Mock<IQueryHandler<GetEmployeesQuery, Result<GetEmployeesResult>>>();

        mockEmployeesHandler
            .Setup(h => h.Handle(It.IsAny<GetEmployeesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fail<GetEmployeesResult>("employees_failed", "Employees query failed."));

        var mockPositionsHandler =
            new Mock<IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>>>();

        // Act
        var result = await GetEmployees.Handle(
            request,
            mockEmployeesHandler.Object,
            mockPositionsHandler.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();

        mockPositionsHandler.Verify(
            h => h.Handle(It.IsAny<GetPositionsQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Problem_When_Positions_Query_Fails()
    {
        // Arrange
        var request = new GetEmployeesRequest(Page: 1, PageSize: 10, Name: "doe");

        var employeesResult = new GetEmployeesResult(
            EmployeesDtos: new List<EmployeeDto>(),
            TotalCount: 0,
            Page: 1,
            PageSize: 10,
            TotalPages: 0
        );

        var mockEmployeesHandler =
            new Mock<IQueryHandler<GetEmployeesQuery, Result<GetEmployeesResult>>>();

        mockEmployeesHandler
            .Setup(h => h.Handle(It.IsAny<GetEmployeesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Ok(employeesResult));

        var mockPositionsHandler =
            new Mock<IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>>>();

        mockPositionsHandler
            .Setup(h => h.Handle(It.IsAny<GetPositionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fail<GetPositionsResult>("positions_failed", "Positions query failed."));

        // Act
        var result = await GetEmployees.Handle(
            request,
            mockEmployeesHandler.Object,
            mockPositionsHandler.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<ProblemHttpResult>();
    }
}
