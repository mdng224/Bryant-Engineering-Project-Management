using App.Application.Common.Dtos;

namespace App.Application.Positions.Queries;

public sealed record GetAllPositionsResult(IReadOnlyList<PositionDto> Positions);