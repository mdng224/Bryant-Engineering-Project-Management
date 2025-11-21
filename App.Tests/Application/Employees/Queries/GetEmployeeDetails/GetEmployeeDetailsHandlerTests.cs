using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Employees;
using App.Application.Employees.Queries.GetEmployeeDetails;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Employees.Queries.GetEmployeeDetails;

public class GetEmployeeDetailsHandlerTests
{
    private readonly Mock<IEmployeeReader> _readerMock = new();

    private GetEmployeeDetailsHandler CreateHandler() => new(_readerMock.Object);

    [Fact]
    public async Task Handle_WhenEmployeeExists_ShouldReturnOkWithDto()
    {
        // Arrange
        var handler = CreateHandler();
        var employeeId = Guid.NewGuid();
        var query = new GetEmployeeDetailsQuery(employeeId);

        var dto = new EmployeeDetailsDto(
            Id: employeeId,
            UserId: null,

            // Identity
            FirstName: "John",
            LastName: "Doe",
            PreferredName: "Johnny",

            // Employment
            EmploymentType: "FullTime",
            SalaryType: "Salary",
            HireDate: DateTimeOffset.UtcNow,
            EndDate: null,

            // Organization
            Department: "Engineering",
            PositionNames: ["Engineer"],

            // Contact / Misc
            CompanyEmail: "john.doe@example.com",
            WorkLocation: "Denver",
            Notes: null,

            // Address
            AddressLine1: "123 Main St",
            AddressLine2: "Suite 100",
            City: "Denver",
            State: "CO",
            PostalCode: "80014",

            // Role recommendation / preapproval
            RecommendRole: null,
            IsPreapproved: false,

            // Auditing — use fake timestamps & IDs for testing
            CreatedAtUtc: DateTimeOffset.UtcNow,
            UpdatedAtUtc: DateTimeOffset.UtcNow,
            DeletedAtUtc: null,
            CreatedById: Guid.NewGuid(),
            UpdatedById: Guid.NewGuid(),
            DeletedById: null
        );

        _readerMock
            .Setup(r => r.GetDetailsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(dto);

        _readerMock.Verify(
            r => r.GetDetailsAsync(employeeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmployeeDoesNotExist_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var handler = CreateHandler();
        var employeeId = Guid.NewGuid();
        var query = new GetEmployeeDetailsQuery(employeeId);

        _readerMock
            .Setup(r => r.GetDetailsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeDetailsDto?)null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");
        result.Error!.Value.Message.Should().Be("Employee not found.");

        _readerMock.Verify(
            r => r.GetDetailsAsync(employeeId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
