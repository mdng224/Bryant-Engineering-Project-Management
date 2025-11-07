using App.Api.Contracts.Positions.Responses;

namespace App.Api.Contracts.Projects.Responses;

// One row in the table
public sealed record ProjectListItemResponse(
    ProjectSummaryResponse Summary,
    ProjectResponse Details
);