using App.Application.Abstractions.Persistence;
using App.Application.Common.Dtos;
using App.Application.EmployeePositions.Queries;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.EmployeePositions.Queries;

public class GetPositionsForEmployeesHandlerTests
    {
        [Fact]
        public async Task Handle_Should_Return_EmptyDictionary_And_NotCallReader_When_NoEmployeeIds()
        {
            // Arrange
            var reader = new Mock<IEmployeePositionReader>(MockBehavior.Strict);
            var handler = new GetPositionsForEmployeesHandler(reader.Object);

            var query = new GetPositionsForEmployeesQuery([]);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Count.Should().Be(0);

            // Reader should NOT be called
            reader.Verify(r => r.GetPositionsForEmployeesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Should_CallReader_And_Return_Map_When_EmployeeIds_Present()
        {
            // Arrange
            var employeeIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var expected = new Dictionary<Guid, IReadOnlyList<PositionMiniDto>>
            {
                [employeeIds[0]] = new List<PositionMiniDto>
                {
                    new(Guid.NewGuid(), "Project Engineer"),
                    new(Guid.NewGuid(), "Senior Drafter"),
                },
                [employeeIds[1]] = new List<PositionMiniDto>
                {
                    new PositionMiniDto(Guid.NewGuid(), "PIC"),
                }
            };

            var reader = new Mock<IEmployeePositionReader>();
            reader
                .Setup(r => r.GetPositionsForEmployeesAsync(
                    It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(employeeIds)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var handler = new GetPositionsForEmployeesHandler(reader.Object);
            var query = new GetPositionsForEmployeesQuery(employeeIds);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            // Compare keys then per-employee position names
            result.Value!.Keys.Should().BeEquivalentTo(expected.Keys);

            foreach (var empId in expected.Keys)
            {
                var actualList = result.Value![empId];
                var expectedList = expected[empId];

                // Same count and same (Id, Name) pairs irrespective of order
                actualList.Should().BeEquivalentTo(expectedList, options => options.WithoutStrictOrdering());
            }

            reader.Verify(r => r.GetPositionsForEmployeesAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(employeeIds)),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Pass_CancellationToken_Through()
        {
            // Arrange
            var employeeIds = new[] { Guid.NewGuid() };
            var cts = new CancellationTokenSource();

            var reader = new Mock<IEmployeePositionReader>();
            reader
                .Setup(r => r.GetPositionsForEmployeesAsync(
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.Is<CancellationToken>(t => t == cts.Token)))
                .ReturnsAsync(new Dictionary<Guid, IReadOnlyList<PositionMiniDto>>());

            var handler = new GetPositionsForEmployeesHandler(reader.Object);
            var query = new GetPositionsForEmployeesQuery(employeeIds);

            // Act
            var _ = await handler.Handle(query, cts.Token);

            // Assert
            reader.Verify(r => r.GetPositionsForEmployeesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.Is<CancellationToken>(t => t == cts.Token)),
                Times.Once);
        }
    }